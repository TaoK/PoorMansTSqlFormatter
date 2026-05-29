using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.CommandBars;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;
using PoorMansTSqlFormatterLib;
using PoorMansTSqlFormatterLib.Formatters;
using Task = System.Threading.Tasks.Task;

namespace PoorMansTSqlFormatterSSMS21
{
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [InstalledProductRegistration("Poor Man's T-SQL Formatter (SSMS 21)",
        "Formateador de T-SQL para SSMS 21", "1.0")]
    [ProvideAutoLoad("{ADFC4E64-0397-11D1-9F4E-00A0C911004F}", PackageAutoLoadFlags.BackgroundLoad)]
    [ProvideAutoLoad("{f1536ef8-92ec-443c-9ed7-fdadf150da82}", PackageAutoLoadFlags.BackgroundLoad)]
    [ProvideOptionPage(typeof(FormatterOptionsPage), "SQL Tools", "T-SQL Formatter", 0, 0, true)]
    [Guid("a1b2c3d4-e5f6-7890-abcd-ef1234567890")]
    public sealed class PoorMansTSqlFormatterSSMS21Package : AsyncPackage
    {
        // Guardamos referencias para que el GC no limpie los event handlers
        private object _formatEvents;
        private object _settingsEvents;

        public FormatterOptionsPage Options =>
            GetDialogPage(typeof(FormatterOptionsPage)) as FormatterOptionsPage;

        protected override async Task InitializeAsync(
            CancellationToken cancellationToken,
            IProgress<ServiceProgressData> progress)
        {
            await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
            try
            {
                var dte = await GetServiceAsync(typeof(DTE)) as DTE2;
                if (dte != null)
                    AddMenuItems(dte);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("[Poor's Man T-SQL Formatter] Init error: " + ex.Message);
            }
        }

        // ── Menú via DTE CommandBars (único mecanismo que funciona en SSMS) ──

        private void AddMenuItems(DTE2 dte)
        {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();

            var bars = (Microsoft.VisualStudio.CommandBars.CommandBars)dte.CommandBars;

            // Buscar el menú Tools
            CommandBarPopup toolsMenu = null;
            foreach (CommandBarControl ctrl in bars["MenuBar"].Controls)
            {
                if (ctrl is CommandBarPopup p &&
                    p.Caption.Replace("&", "").Trim()
                             .Equals("Tools", StringComparison.OrdinalIgnoreCase))
                {
                    toolsMenu = p; break;
                }
            }
            if (toolsMenu == null) return;

            // Guard: si el submenú ya existe, no lo volvemos a crear
            // (evita duplicados si el paquete se reinicializa en la misma sesión)
            foreach (CommandBarControl ctrl in toolsMenu.Controls)
            {
                if (ctrl is CommandBarPopup existing &&
                    existing.Caption == "Poor's Man T-SQL Formatter") return;
            }

            // Submenú "T-SQL Formatter" dentro de Tools
            var subMenu = (CommandBarPopup)toolsMenu.Controls.Add(
                MsoControlType.msoControlPopup,
                Type.Missing, Type.Missing, Type.Missing, /*Temporary=*/true);
            subMenu.Caption    = "Poor's Man T-SQL Formatter";
            subMenu.BeginGroup = true;

            // Botón: Format T-SQL Code
            var btnFormat = (CommandBarButton)subMenu.Controls.Add(
                MsoControlType.msoControlButton,
                Type.Missing, Type.Missing, Type.Missing, true);
            btnFormat.Caption     = "Format T-SQL Code";
            btnFormat.TooltipText = "Formatea el SQL seleccionado (o todo el editor)";

            var fmtEvents = (CommandBarEvents)dte.Events.get_CommandBarEvents(btnFormat);
            _formatEvents = fmtEvents;
            fmtEvents.Click += (object _, ref bool __, ref bool ___) =>
            {
                Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
                ExecuteFormat();
            };

            // Botón: Settings...
            var btnSettings = (CommandBarButton)subMenu.Controls.Add(
                MsoControlType.msoControlButton,
                Type.Missing, Type.Missing, Type.Missing, true);
            btnSettings.Caption     = "Settings...";
            btnSettings.TooltipText = "Configurar opciones del formateador";
            btnSettings.BeginGroup  = true;

            var settEvents = (CommandBarEvents)dte.Events.get_CommandBarEvents(btnSettings);
            _settingsEvents = settEvents;
            settEvents.Click += (object _, ref bool __, ref bool ___) =>
            {
                Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
                ShowOptionPage(typeof(FormatterOptionsPage));
            };
        }

        // ── Lógica de formateo ────────────────────────────────────────────────

        private void ExecuteFormat()
        {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
            try
            {
                if (!TryGetEditorContent(out string sql, out Action<string> replace, out _))
                    return;

                var o   = Options ?? new FormatterOptionsPage();
                var fmt = new TSqlStandardFormatter(
                    o.IndentString, o.SpacesPerTab, o.MaxLineWidth,
                    o.ExpandCommaLists, o.TrailingCommas, o.SpaceAfterExpandedComma,
                    o.ExpandBooleanExpressions, o.ExpandCaseStatements,
                    o.ExpandBetweenConditions, o.BreakJoinOnSections,
                    o.UppercaseKeywords, false, o.KeywordStandardization);

                bool errorFound = false;
                string result = new SqlFormattingManager(fmt).Format(sql, ref errorFound);

                if (errorFound &&
                    MessageBox.Show(
                        "El formateador encontró construcciones SQL que no reconoce.\n¿Aplicar igual?",
                        "Poor's Man T-SQL Formatter", MessageBoxButtons.YesNo, MessageBoxIcon.Warning)
                    != DialogResult.Yes)
                    return;

                replace(result);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Poor's Man T-SQL Formatter",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ── Acceso al editor activo vía VS Shell ──────────────────────────────

        private bool TryGetEditorContent(
            out string sql, out Action<string> replacer, out bool hasSel)
        {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
            sql = ""; replacer = _ => { }; hasSel = false;

            var tm = Package.GetGlobalService(typeof(SVsTextManager)) as IVsTextManager2;
            if (tm == null) return false;

            tm.GetActiveView2(1, null, (uint)_VIEWFRAMETYPE.vftCodeWindow, out IVsTextView tv);
            if (tv == null) return false;

            tv.GetBuffer(out IVsTextLines buf);
            if (buf == null) return false;

            tv.GetSelection(out int sl, out int sc, out int el, out int ec);

            if (sl != el || sc != ec)
            {
                hasSel = true;
                buf.GetLineText(sl, sc, el, ec, out sql);
                int a = sl, b = sc, c = el, d = ec;
                replacer = t =>
                {
                    Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
                    Replace(buf, a, b, c, d, t);
                };
            }
            else
            {
                buf.GetLastLineIndex(out int ll, out int lc);
                buf.GetLineText(0, 0, ll, lc, out sql);
                replacer = t =>
                {
                    Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
                    Replace(buf, 0, 0, ll, lc, t);
                };
            }
            return true;
        }

        private static void Replace(IVsTextLines buf,
            int sl, int sc, int el, int ec, string text)
        {
            IntPtr ptr = Marshal.StringToCoTaskMemUni(text);
            try   { buf.ReplaceLines(sl, sc, el, ec, ptr, text.Length, null); }
            finally { Marshal.FreeCoTaskMem(ptr); }
        }
    }
}
