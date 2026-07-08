using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;
using CreamInstaller.Components;
using CreamInstaller.Utility;
using HtmlAgilityPack;
using Onova;
using Onova.Models;
using Onova.Services;

namespace CreamInstaller.Forms;

internal sealed partial class MainForm : CustomForm
{
    private CancellationTokenSource cancellationTokenSource;
    private Version latestVersion;
    private SelectForm selectForm;

    private UpdateManager updateManager;
    private IReadOnlyList<Version> versions;

    internal MainForm()
    {
        InitializeComponent();
        Text = Program.ApplicationNameShort;
        headerLabel.Text = Program.ApplicationNameShort;
    }

    private void StartProgram(bool cancelUpdateCheck = true)
    {
        if (cancelUpdateCheck && cancellationTokenSource is not null)
        {
            cancellationTokenSource.Cancel();
            cancellationTokenSource.Dispose();
            cancellationTokenSource = null;
        }
        if (selectForm is null)
        {
#pragma warning disable CA2000 // Dispose objects before losing scope
            selectForm = new SelectForm();
#pragma warning restore CA2000 // Dispose objects before losing scope
            selectForm.InheritLocation(this);
            selectForm.FormClosing += (_, _) => Close();
            selectForm.Show();
#if DEBUG
            DebugForm.Current.Attach(selectForm);
#endif
        }
        Hide();
    }

    private async void OnLoad()
    {
        progressBar.Visible   = false;
        ignoreButton.Visible  = true;
        updateButton.Text     = "Update";
        updateButton.IsAccent = true;
        updateButton.Click   -= OnUpdateCancel;
        progressLabel.Text    = "Checking for updates . . .";
        progressLabel.ForeColor = Components.ThemeManager.TextSecondary;
        changelogTreeView.Visible  = false;
        changelogTreeView.Location = progressLabel.Location with { Y = progressLabel.Location.Y + progressLabel.Size.Height + 13 };
        Refresh();
#if DEBUG
        DebugForm.Current.Attach(this);
#endif
        GithubPackageResolver resolver = new(Program.RepositoryOwner, Program.RepositoryName, Program.RepositoryPackage);
        ZipPackageExtractor extractor = new();
        updateManager = new(AssemblyMetadata.FromAssembly(Program.EntryAssembly, Program.CurrentProcessFilePath), resolver, extractor);
        if (latestVersion is null)
        {
            cancellationTokenSource = new();
            cancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(30));
            try
            {
                Task<CheckForUpdatesResult> checkForUpdatesTask = updateManager.CheckForUpdatesAsync(cancellationTokenSource.Token);
                // Don't hold the program hostage to the network: if the check isn't done
                // within the grace period, open the game list and let it finish in the background.
                if (await Task.WhenAny(checkForUpdatesTask, Task.Delay(2000)) != checkForUpdatesTask)
                    StartProgram(cancelUpdateCheck: false);
                CheckForUpdatesResult checkForUpdatesResult = await checkForUpdatesTask;
#if !DEBUG
                if (checkForUpdatesResult.CanUpdate)
                {
#endif
                latestVersion = checkForUpdatesResult.LastVersion;
                versions = checkForUpdatesResult.Versions;
#if !DEBUG
                }
#endif
            }
#if DEBUG
            catch (TaskCanceledException) { }
            catch (Exception e)
            {
                DebugForm.Current.Log($"Exception while checking for updates: {e.GetType()} ({e.Message})", LogTextBox.Warning);
            }
#else
            catch
            {
                // ignored
            }
#endif
            finally
            {
                cancellationTokenSource?.Dispose();
                cancellationTokenSource = null;
            }
        }
        if (IsDisposed)
            return; // the program was closed while the background check was still running
        if (latestVersion is null)
        {
            updateManager.Dispose();
            updateManager = null;
            StartProgram();
        }
        else
        {
            progressLabel.Text      = $"An update is available: v{latestVersion}";
            progressLabel.ForeColor = Components.ThemeManager.Warning;
            ignoreButton.Enabled    = true;
            updateButton.Enabled    = true;
            updateButton.Click     += OnUpdate;
            changelogTreeView.Visible = true;
            if (!Visible)
                Show(); // the program was started while the check was still running; resurface the update prompt
            Version currentVersion = new(Program.Version);
#if DEBUG
            foreach (Version version in versions.Where(v => (v > currentVersion || v == latestVersion) && !changelogTreeView.Nodes.ContainsKey(v.ToString())))
#else
            foreach (Version version in versions.Where(v => v > currentVersion && !changelogTreeView.Nodes.ContainsKey(v.ToString())))
#endif
            {
                TreeNode root = new($"v{version}") { Name = version.ToString() };
                changelogTreeView.Nodes.Add(root);
                if (changelogTreeView.Nodes.Count > 0)
                    changelogTreeView.Nodes[0].EnsureVisible();
                _ = Task.Run(async () =>
                {
                    HtmlNodeCollection nodes = await HttpClientManager.GetDocumentNodes(
                        $"https://github.com/{Program.RepositoryOwner}/{Program.RepositoryName}/releases/tag/v{version}",
                        "//div[@data-test-selector='body-content']/ul/li");
                    if (nodes is null)
                        changelogTreeView.Nodes.Remove(root);
                    else
                        foreach (HtmlNode node in nodes)
                            changelogTreeView.Invoke(delegate
                            {
                                TreeNode change = new() { Text = HttpUtility.HtmlDecode(node.InnerText) ?? string.Empty };
                                root.Nodes.Add(change);
                                root.Expand();
                                if (changelogTreeView.Nodes.Count > 0)
                                    changelogTreeView.Nodes[0].EnsureVisible();
                            });
                });
            }
        }
    }

    private void OnLoad(object sender, EventArgs _)
    {
    retry:
        try
        {
            string fileName = Path.GetFileName(Program.CurrentProcessFilePath);
            if (fileName != Program.ApplicationExecutable)
            {
                using DialogForm form = new(this);
                if (form.Show(SystemIcons.Warning,
                        "WARNING: " + Program.ApplicationExecutable + " was renamed!" + "\n\nThis will cause undesirable behavior when updating the program!",
                        "Ignore", "Abort") == DialogResult.Cancel)
                {
                    Application.Exit();
                    return;
                }
            }
            OnLoad();
        }
        catch (Exception e)
        {
            if (e.HandleException(this))
                goto retry;
            Close();
        }
    }

    private void OnIgnore(object sender, EventArgs e) => StartProgram();

    private async void OnUpdate(object sender, EventArgs e)
    {
        progressBar.Visible   = true;
        ignoreButton.Visible  = false;
        updateButton.Text     = "Cancel";
        updateButton.IsAccent = false;
        updateButton.Click   -= OnUpdate;
        updateButton.Click   += OnUpdateCancel;
        changelogTreeView.Location = progressBar.Location with { Y = progressBar.Location.Y + progressBar.Size.Height + 6 };
        Refresh();
        Progress<double> progress = new();
        progress.ProgressChanged += delegate(object _, double _progress)
        {
            progressLabel.Text      = $"Updating . . . {(int)_progress}%";
            progressLabel.ForeColor = Components.ThemeManager.Accent;
            progressBar.Value       = (int)_progress;
        };
        progressLabel.Text = "Updating . . . ";
        cancellationTokenSource = new();
        try
        {
            await updateManager.PrepareUpdateAsync(latestVersion, progress, cancellationTokenSource.Token);
        }
#if DEBUG
        catch (TaskCanceledException) { }
        catch (Exception ex)
        {
            DebugForm.Current.Log($"Exception while preparing update: {ex.GetType()} ({ex.Message})", LogTextBox.Warning);
        }
#else
        catch
        {
            // ignored
        }
#endif
        finally
        {
            cancellationTokenSource.Dispose();
            cancellationTokenSource = null;
        }
        if (updateManager is not null && updateManager.IsUpdatePrepared(latestVersion))
        {
            updateManager.LaunchUpdater(latestVersion);
            Application.Exit();
            return;
        }
        OnLoad();
    }

    private void OnUpdateCancel(object sender, EventArgs e)
    {
        cancellationTokenSource?.Cancel();
        updateManager?.Dispose();
        updateManager = null;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
            components?.Dispose();
        base.Dispose(disposing);
        cancellationTokenSource?.Dispose();
        updateManager?.Dispose();
    }
}