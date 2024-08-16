#pragma warning disable CS1591
// ReSharper disable ClassNeverInstantiated.Global
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Build;
using Cake.Common;
using Cake.Common.IO;
using Cake.Common.Tools.DotNet;
using Cake.Common.Tools.DotNet.Build;
using Cake.Common.Tools.DotNet.MSBuild;
using Cake.Common.Tools.DotNet.NuGet.Push;
using Cake.Core;
using Cake.Core.Diagnostics;
using Cake.Core.IO;
using Cake.Frosting;
using Cake.Git;
using Cake.Json;
using Microsoft.Build.Definition;
using Microsoft.Build.Evaluation;

return new CakeHost()
       .UseContext<BuildContext>()
       .Run(args);

namespace Build
{
    public class BuildContext : FrostingContext
    {
        public enum ProjectBuildType
        {
            Release,
            Development,
            BleedingEdge
        }

        public const string DOORSTOP_VERSION = "4.3.0";
        public const string DOTNET_RUNTIME_VERSION = "6.0.7";
        public const string DOBBY_VERSION = "1.0.5";

        public const string DOTNET_RUNTIME_ZIP_URL =
            $"https://github.com/BepInEx/dotnet-runtime/releases/download/{DOTNET_RUNTIME_VERSION}/mini-coreclr-Release.zip";

        internal readonly DistributionTarget[] Distributions =
        [
            new DistributionTarget("Unity.IL2CPP", "win-x86")
        ];


        public BuildContext(ICakeContext ctx)
            : base(ctx)
        {
            RootDirectory = ctx.Environment.WorkingDirectory.GetParent();
            OutputDirectory = RootDirectory.Combine("bin");
            CacheDirectory = OutputDirectory.Combine(".dep_cache");
            DistributionDirectory = OutputDirectory.Combine("dist");
            var props = Project.FromFile(RootDirectory.CombineWithFilePath("Directory.Build.props").FullPath,
                                         new ProjectOptions());
            VersionPrefix = props.GetPropertyValue("VersionPrefix");
            CurrentCommit = ctx.GitLogTip(RootDirectory);

            BuildType = ctx.Argument("build-type", ProjectBuildType.Development);
            BuildId = ctx.Argument("build-id", -1);
            LastBuildCommit = ctx.Argument("last-build-commit", "");
            NugetApiKey = ctx.Argument("nuget-api-key", "");
            NugetSource = ctx.Argument("nuget-source", "https://nuget.bepinex.dev/v3/index.json");
        }

        public ProjectBuildType BuildType { get; }
        public int BuildId { get; }
        public string LastBuildCommit { get; }
        public string NugetApiKey { get; }
        public string NugetSource { get; }

        public DirectoryPath RootDirectory { get; }
        public DirectoryPath OutputDirectory { get; }
        public DirectoryPath CacheDirectory { get; }
        public DirectoryPath DistributionDirectory { get; }

        public string VersionPrefix { get; }
        public GitCommit CurrentCommit { get; }

        public string VersionSuffix => BuildType switch
        {
            ProjectBuildType.Release      => "",
            ProjectBuildType.Development  => "dev",
            ProjectBuildType.BleedingEdge => $"be.{BuildId}",
            var _                         => throw new ArgumentOutOfRangeException()
        };

        public string BuildPackageVersion =>
            VersionPrefix + BuildType switch
            {
                ProjectBuildType.Release => "",
                var _                    => $"-{VersionSuffix}+{this.GitShortenSha(RootDirectory, CurrentCommit)}"
            };

        public static string DoorstopZipUrl(string arch) =>
            $"https://github.com/NeighTools/UnityDoorstop/releases/download/v{DOORSTOP_VERSION}/doorstop_{arch}_release_{DOORSTOP_VERSION}.zip";

        public static string DobbyZipUrl(string arch) =>
            $"https://github.com/BepInEx/Dobby/releases/download/v{DOBBY_VERSION}/dobby-{arch}.zip";
    }

    [TaskName("Clean")]
    public sealed class CleanTask : FrostingTask<BuildContext>
    {
        public override void Run(BuildContext ctx)
        {
            ctx.CreateDirectory(ctx.OutputDirectory);
            ctx.CleanDirectory(ctx.OutputDirectory,
                               f => !f.Path.FullPath.Contains(".dep_cache"));

            ctx.Log.Information("Cleaning up old build objects");
            ctx.CleanDirectories(ctx.RootDirectory.Combine("**/BepInEx.*/**/bin").FullPath);
            ctx.CleanDirectories(ctx.RootDirectory.Combine("**/BepInEx.*/**/obj").FullPath);
        }
    }

    [TaskName("Compile")]
    [IsDependentOn(typeof(CleanTask))]
    public sealed class CompileTask : FrostingTask<BuildContext>
    {
        public override void Run(BuildContext ctx)
        {
            var buildSettings = new DotNetBuildSettings
            {
                Configuration = "Release"
            };
            if (ctx.BuildType != BuildContext.ProjectBuildType.Release)
                buildSettings.MSBuildSettings = new DotNetMSBuildSettings
                {
                    VersionSuffix = ctx.VersionSuffix,
                    Properties =
                    {
                        ["SourceRevisionId"] = new[] { ctx.CurrentCommit.Sha },
                        ["RepositoryBranch"] = new[] { ctx.GitBranchCurrent(ctx.RootDirectory).FriendlyName }
                    }
                };

            ctx.DotNetBuild(ctx.RootDirectory.FullPath, buildSettings);
        }
    }

    [TaskName("DownloadDependencies")]
    public sealed class DownloadDependenciesTask : FrostingTask<BuildContext>
    {
        public override void Run(BuildContext ctx)
        {
            ctx.Log.Information("Downloading dependencies");
            ctx.CreateDirectory(ctx.CacheDirectory);

            var cache = new DependencyCache(ctx, ctx.CacheDirectory.CombineWithFilePath("cache.json"));

            cache.Refresh("NeighTools/UnityDoorstop", BuildContext.DOORSTOP_VERSION, () =>
            {
                ctx.Log.Information($"Downloading Doorstop {BuildContext.DOORSTOP_VERSION}");
                var doorstopDir = ctx.CacheDirectory.Combine("doorstop");
                ctx.CreateDirectory(doorstopDir);
                ctx.CleanDirectory(doorstopDir);
                var archs = new[] { "win", "linux", "macos" };
                var versions = archs
                               .Select(a => ($"Doorstop ({a})",
                                             BuildContext.DoorstopZipUrl(a),
                                             doorstopDir.Combine($"doorstop_{a}")))
                               .ToArray();
                ctx.DownloadZipFiles($"Doorstop {BuildContext.DOORSTOP_VERSION}", versions);
            });

            cache.Refresh("BepInEx/Dobby", BuildContext.DOBBY_VERSION, () =>
            {
                ctx.Log.Information($"Downloading Dobby {BuildContext.DOBBY_VERSION}");
                var dobbyDir = ctx.CacheDirectory.Combine("dobby");
                ctx.CreateDirectory(dobbyDir);
                ctx.CleanDirectory(dobbyDir);
                var archs = new[] { "win", "linux", "macos" };
                var versions = archs
                               .Select(a => ($"Dobby ({a})", BuildContext.DobbyZipUrl(a),
                                             dobbyDir.Combine($"dobby_{a}")))
                               .ToArray();
                ctx.DownloadZipFiles($"Dobby {BuildContext.DOBBY_VERSION}", versions);
            });

            cache.Refresh("BepInEx/dotnet_runtime", BuildContext.DOTNET_RUNTIME_VERSION, () =>
            {
                ctx.Log.Information($"Downloading dotnet runtime {BuildContext.DOTNET_RUNTIME_VERSION}");
                var dotnetDir = ctx.CacheDirectory.Combine("dotnet");
                ctx.CreateDirectory(dotnetDir);
                ctx.CleanDirectory(dotnetDir);
                ctx.DownloadZipFiles($"dotnet-runtime {BuildContext.DOTNET_RUNTIME_VERSION}",
                                     ("dotnet runtime", BuildContext.DOTNET_RUNTIME_ZIP_URL, dotnetDir));
            });

            cache.Save();
        }
    }

    [TaskName("MakeDist")]
    [IsDependentOn(typeof(CompileTask))]
    [IsDependentOn(typeof(DownloadDependenciesTask))]
    public sealed class MakeDistTask : FrostingTask<BuildContext>
    {
        public override void Run(BuildContext ctx)
        {
            ctx.CreateDirectory(ctx.DistributionDirectory);
            ctx.CleanDirectory(ctx.DistributionDirectory);

            var latestTag = ctx.Git("describe --tags --abbrev=0");
            var changelog = new StringBuilder()
                            .AppendLine(
                                        $"{ctx.Git($"rev-list --count {latestTag}..HEAD")} changes since {latestTag}")
                            .AppendLine()
                            .AppendLine("Changelog (excluding merge commits):")
                            .AppendLine(ctx.Git(
                                                $"--no-pager log --no-merges --pretty=\"format:* (%h) [%an] %s\" {latestTag}..HEAD",
                                                Environment.NewLine))
                            .ToString();


            foreach (var dist in ctx.Distributions)
            {
                ctx.Log.Information($"Creating distribution {dist.Target}");
                var targetDir = ctx.DistributionDirectory.Combine(dist.Target);
                ctx.CreateDirectory(targetDir);
                ctx.CleanDirectory(targetDir);

                var bepInExDir = targetDir.Combine("BepInEx");
                var bepInExCoreDir = bepInExDir.Combine("core");
                ctx.CreateDirectory(bepInExDir);
                ctx.CreateDirectory(bepInExCoreDir);
                ctx.CreateDirectory(bepInExDir.Combine("plugins"));
                ctx.CreateDirectory(bepInExDir.Combine("patchers"));

                File.WriteAllText(targetDir.CombineWithFilePath("changelog.txt").FullPath, changelog);

                var sourceDirectory = ctx.OutputDirectory.Combine(dist.DistributionIdentifier);
                if (dist.FrameworkTarget != null)
                    sourceDirectory = sourceDirectory.Combine(dist.FrameworkTarget);

                foreach (var filePath in ctx.GetFiles(sourceDirectory.Combine("*.*").FullPath))
                    ctx.CopyFileToDirectory(filePath, bepInExCoreDir);

                if (dist.Engine == "Unity")
                {
                    var doorstopPath =
                        ctx.CacheDirectory.Combine("doorstop").Combine($"doorstop_{dist.Os}").Combine(dist.Arch);
                    foreach (var filePath in ctx.GetFiles(doorstopPath.Combine($"*.{dist.DllExtension}").FullPath))
                        ctx.CopyFileToDirectory(filePath, targetDir);
                    ctx.CopyFileToDirectory(doorstopPath.CombineWithFilePath(".doorstop_version"), targetDir);
                    var (doorstopConfigFile, doorstopConfigDistName) = dist.Os switch
                    {
                        "win" => ($"doorstop_config_{dist.Runtime.ToLower()}.ini",
                                  "doorstop_config.ini"),
                        "linux" or "macos" => ($"run_bepinex_{dist.Runtime.ToLower()}.sh",
                                               "run_bepinex.sh"),
                        var _ => throw new
                                     NotSupportedException(
                                                           $"Doorstop is not supported on {dist.Os}")
                    };
                    ctx.CopyFile(ctx.RootDirectory.Combine("Runtimes").Combine("Unity").Combine("Doorstop").CombineWithFilePath(doorstopConfigFile),
                                 targetDir.CombineWithFilePath(doorstopConfigDistName));

                    if (dist.Runtime == "IL2CPP")
                    {
                        ctx.CopyFile(ctx.CacheDirectory.Combine("dobby").Combine($"dobby_{dist.Os}").CombineWithFilePath($"{dist.DllPrefix}dobby_{dist.Arch}.{dist.DllExtension}"),
                                     bepInExCoreDir.CombineWithFilePath($"{dist.DllPrefix}dobby.{dist.DllExtension}"));
                        ctx.CopyDirectory(ctx.CacheDirectory.Combine("dotnet").Combine(dist.RuntimeIdentifier),
                                          targetDir.Combine("dotnet"));
                    }
                }
                else if (dist.Engine == "NET")
                {
                    if (dist.Runtime == "Framework")
                    {
                        ctx.DeleteFile(bepInExCoreDir.CombineWithFilePath("BepInEx.NET.Framework.Launcher.exe.config"));

                        ctx.MoveFileToDirectory(bepInExCoreDir.CombineWithFilePath("BepInEx.NET.Framework.Launcher.exe"),
                                                targetDir);
                    }
                    else if (dist.Runtime == "CoreCLR")
                    {
                        foreach (var filePath in ctx.GetFiles(bepInExCoreDir.Combine("BepInEx.NET.CoreCLR.*").FullPath))
                            ctx.MoveFileToDirectory(filePath, targetDir);
                    }
                }
            }
        }
    }

    [TaskName("PushNuGet")]
    public sealed class PushNuGetTask : FrostingTask<BuildContext>
    {
        public override bool ShouldRun(BuildContext ctx) => !string.IsNullOrWhiteSpace(ctx.NugetApiKey) &&
                                                            ctx.BuildType != BuildContext.ProjectBuildType.Development;

        public override void Run(BuildContext ctx)
        {
            var nugetPath = ctx.OutputDirectory.Combine("NuGet");
            var settings = new DotNetNuGetPushSettings
            {
                Source = ctx.NugetSource,
                ApiKey = ctx.NugetApiKey
            };
            foreach (var pkg in ctx.GetFiles(nugetPath.Combine("*.nupkg").FullPath))
                ctx.DotNetNuGetPush(pkg, settings);
        }
    }

    [TaskName("Publish")]
    [IsDependentOn(typeof(MakeDistTask))]
    [IsDependentOn(typeof(PushNuGetTask))]
    public sealed class PublishTask : FrostingTask<BuildContext>
    {
        public override void Run(BuildContext ctx)
        {
            ctx.Log.Information("Packing BepInEx");

            foreach (var dist in ctx.Distributions)
            {
                var targetZipName = $"BepInEx-{dist.Target}-{ctx.BuildPackageVersion}.zip";
                ctx.Log.Information($"Packing {targetZipName}");
                ctx.Zip(ctx.DistributionDirectory.Combine(dist.Target),
                        ctx.DistributionDirectory
                           .CombineWithFilePath(targetZipName));
            }


            var changeLog = "";
            if (!string.IsNullOrWhiteSpace(ctx.LastBuildCommit))
            {
                var changeLogContents =
                    ctx.Git($"--no-pager log --no-merges --pretty=\"format:<li>(<code>%h</code>) [%an] %s</li>\" {ctx.LastBuildCommit}..HEAD",
                            "\n");
                changeLog = $"<ul>{changeLogContents}</ul>";
            }

            ctx.SerializeJsonToPrettyFile(ctx.DistributionDirectory.CombineWithFilePath("info.json"),
                                          new Dictionary<string, object>
                                          {
                                              ["id"] = ctx.BuildId.ToString(),
                                              ["date"] = DateTime.Now.ToString("o"),
                                              ["changelog"] = changeLog,
                                              ["hash"] = ctx.CurrentCommit.Sha,
                                              ["short_hash"] = ctx.GitShortenSha(ctx.RootDirectory, ctx.CurrentCommit),
                                              ["artifacts"] = ctx.Distributions
                                                                 .Select(d => new Dictionary<string, string>
                                                                 {
                                                                     ["file"] =
                                                                         $"BepInEx-{d.Target}-{ctx.BuildPackageVersion}.zip",
                                                                     ["description"] =
                                                                         $"BepInEx {d.Engine} ({d.Runtime}{(d.FrameworkTarget == null ? "" : " " + d.FrameworkTarget)}) for {d.ClearOsName} ({d.Arch}) games"
                                                                 }).ToArray()
                                          });
        }
    }

    [TaskName("Default")]
    [IsDependentOn(typeof(CompileTask))]
    public class DefaultTask : FrostingTask { }
}
