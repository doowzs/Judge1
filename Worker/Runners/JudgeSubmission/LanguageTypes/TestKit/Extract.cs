﻿using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared.Models;
using Worker.Models;

namespace Worker.Runners.JudgeSubmission.LanguageTypes.TestKit
{
    public sealed partial class TestKitRunner
    {
        private async Task<JudgeResult> ExtractAsync()
        {
            try
            {
                var filename = Encoding.UTF8.GetString(Convert.FromBase64String(_submission.Program.Code));
                var archive = Path.Combine(DataPath, "submissions", _submission.Id.ToString(), filename);
                await Task.Run(() => ZipFile.ExtractToDirectory(archive, Path.Combine(Root, "extract")));
            }
            catch (FileNotFoundException)
            {
                return JudgeResult.NewFailedResult("E: Could not find submission archive on server. Is it deleted?");
            }
            catch (InvalidDataException)
            {
                return JudgeResult.NewRejectedResult("E: The zip archive is corrupted and cannot be extracted.");
            }
            catch (Exception e)
            {
                return JudgeResult.NewFailedResult($"E: Failed to extract zip archive: {e.Message}");
            }

            var info = new DirectoryInfo(Path.Combine(Root, "extract"));
            var target = string.Empty;
            if (info.GetDirectories().Any(d => d.Name.Equals(".git")))
            {
                target = Path.Combine(Root, "extract", ".git");
            }
            else if (info.GetDirectories().Length == 1)
            {
                var folder = info.GetDirectories()[0].Name;
                target = Path.Combine(Root, "extract", folder, ".git");
                if (!Directory.Exists(target))
                {
                    return JudgeResult.NewRejectedResult("E: Could not find git repository." +
                                                         $" There is no .git in folder {folder}.");
                }
            }
            else
            {
                return JudgeResult.NewRejectedResult("E: Could not find git repository." +
                                                     " There is none or more than one folders in zip archive.");
            }

            Directory.Move(target, Path.Combine(Jail, ".git"));
            return null;
        }

        private async Task<JudgeResult> RestoreGitRepositoryAsync()
        {
            int result;
            string message;

            result = await _box.ExecuteAsync(
                "/usr/bin/git rev-list --count HEAD",
                bind: new[] {"/etc"},
                chroot: "jail",
                stdout: "git_output",
                proc: 5,
                time: 3.0f,
                memory: 512000
            );
            if (result != 0)
            {
                return JudgeResult.NewRejectedResult("Submitted archive does not contain a git repository.");
            }

            if (int.TryParse(await _box.ReadFileAsync("git_output"), out var count))
            {
                message = $"Found {count} traced git commits.\n";
            }
            else
            {
                return JudgeResult.NewFailedResult("E: Failed to parse result of `git rev-list --count HEAD`.");
            }

            result = await _box.ExecuteAsync(
                "/usr/bin/git reset --hard HEAD",
                bind: new[] {"/etc"},
                chroot: "jail",
                stdout: "git_output",
                proc: 5,
                time: 3.0f,
                memory: 512000
            );
            if (result != 0)
            {
                return JudgeResult.NewRejectedResult("E: Cannot restore git repository to last commit.");
            }

            var version = await _box.ReadFileAsync("git_output");
            if (version.Length < 14)
            {
                return JudgeResult.NewFailedResult("E: Failed to parse result of `git reset --hard HEAD`.");
            }

            return new JudgeResult
            {
                Verdict = Verdict.Accepted,
                Time = null,
                Memory = null,
                Score = 0,
                Message = message + $"Restored repository to commit {version.Substring(15, 7)}."
            };
        }
    }
}