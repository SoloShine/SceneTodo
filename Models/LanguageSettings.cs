namespace SceneTodo.Models
{
    /// <summary>
    /// 语言设置
    /// </summary>
    public class LanguageSettings
    {
        /// <summary>
        /// 当前语言
        /// </summary>
        public SupportedLanguage CurrentLanguage { get; set; } = SupportedLanguage.ChineseSimplified;

        /// <summary>
        /// 是否自动检测系统语言
        /// </summary>
        public bool AutoDetectLanguage { get; set; } = false;
    }
}
