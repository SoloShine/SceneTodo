using System.ComponentModel;

namespace SceneTodo.Models
{
    /// <summary>
    /// 支持的语言
    /// </summary>
    public enum SupportedLanguage
    {
        /// <summary>
        /// 中文（简体）
        /// </summary>
        [Description("中文")]
        ChineseSimplified,

        /// <summary>
        /// English
        /// </summary>
        [Description("English")]
        English
    }
}
