using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using System.Windows.Media;

namespace SceneTodo.Models
{
    /// <summary>
    /// 标签实体
    /// </summary>
    public class Tag : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private string id = Guid.NewGuid().ToString();
        public string Id
        {
            get => id;
            set
            {
                if (id != value)
                {
                    id = value;
                    OnPropertyChanged(nameof(Id));
                }
            }
        }

        private string name = string.Empty;
        /// <summary>
        /// 标签名称
        /// </summary>
        public string Name
        {
            get => name;
            set
            {
                if (name != value)
                {
                    name = value;
                    OnPropertyChanged(nameof(Name));
                }
            }
        }

        private string color = "#2196F3";
        /// <summary>
        /// 标签颜色（十六进制）
        /// </summary>
        public string Color
        {
            get => color;
            set
            {
                if (color != value)
                {
                    color = value;
                    OnPropertyChanged(nameof(Color));
                    OnPropertyChanged(nameof(ColorBrush));
                }
            }
        }

        /// <summary>
        /// 颜色画刷（用于绑定，不存储到数据库）
        /// </summary>
        [NotMapped]
        [JsonIgnore]
        public SolidColorBrush ColorBrush
        {
            get
            {
                try
                {
                    return new SolidColorBrush((Color)ColorConverter.ConvertFromString(Color));
                }
                catch
                {
                    return new SolidColorBrush(Colors.Gray);
                }
            }
        }

        private DateTime? createdAt = DateTime.Now;
        public DateTime? CreatedAt
        {
            get => createdAt;
            set
            {
                if (createdAt != value)
                {
                    createdAt = value;
                    OnPropertyChanged(nameof(CreatedAt));
                }
            }
        }

        /// <summary>
        /// 使用此标签的待办数量（不存储到数据库）
        /// </summary>
        [NotMapped]
        [JsonIgnore]
        public int UsageCount { get; set; }
    }
}
