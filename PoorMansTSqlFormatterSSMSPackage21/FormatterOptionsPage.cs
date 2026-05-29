using System.ComponentModel;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;

namespace PoorMansTSqlFormatterSSMS21
{
    [Guid("B4C5D6E7-F8A9-4B2C-9D3E-F456789012AB")]   // ← GUID real único para esta página
    public class FormatterOptionsPage : DialogPage
    {
        [Category("Indentación")]
        [DisplayName("Cadena de indentación")]
        [DefaultValue("\t")]
        public string IndentString { get; set; } = "\t";

        [Category("Indentación")]
        [DisplayName("Espacios por tabulación")]
        [DefaultValue(4)]
        public int SpacesPerTab { get; set; } = 4;

        [Category("Indentación")]
        [DisplayName("Ancho máximo de línea")]
        [DefaultValue(999)]
        public int MaxLineWidth { get; set; } = 999;

        [Category("Listas")]
        [DisplayName("Expandir listas de comas")]
        [DefaultValue(true)]
        public bool ExpandCommaLists { get; set; } = true;

        [Category("Listas")]
        [DisplayName("Comas al final")]
        [DefaultValue(false)]
        public bool TrailingCommas { get; set; } = false;

        [Category("Listas")]
        [DisplayName("Espacio después de coma")]
        [DefaultValue(true)]
        public bool SpaceAfterExpandedComma { get; set; } = true;

        [Category("Expresiones")]
        [DisplayName("Expandir AND/OR")]
        [DefaultValue(true)]
        public bool ExpandBooleanExpressions { get; set; } = true;

        [Category("Expresiones")]
        [DisplayName("Expandir CASE")]
        [DefaultValue(true)]
        public bool ExpandCaseStatements { get; set; } = true;

        [Category("Expresiones")]
        [DisplayName("Expandir BETWEEN")]
        [DefaultValue(true)]
        public bool ExpandBetweenConditions { get; set; } = true;

        [Category("JOINs")]
        [DisplayName("Salto de línea en ON")]
        [DefaultValue(false)]
        public bool BreakJoinOnSections { get; set; } = false;

        [Category("Keywords")]
        [DisplayName("Palabras clave en MAYÚSCULAS")]
        [DefaultValue(true)]
        public bool UppercaseKeywords { get; set; } = true;

        [Category("Keywords")]
        [DisplayName("Estandarizar keywords")]
        [DefaultValue(false)]
        public bool KeywordStandardization { get; set; } = false;
    }
}