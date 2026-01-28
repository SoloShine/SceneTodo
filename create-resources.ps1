# 资源文件创建辅助脚本
# 使用方法：在 PowerShell 中运行此脚本来创建资源文件

# 确保 Resources 目录存在
$resourceDir = "Resources"
if (-not (Test-Path $resourceDir)) {
    New-Item -Path $resourceDir -ItemType Directory
}

# 中文资源文件模板
$chineseResxTemplate = @'
<?xml version="1.0" encoding="utf-8"?>
<root>
  <xsd:schema id="root" xmlns="" xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:msdata="urn:schemas-microsoft-com:xml-msdata">
    <xsd:import namespace="http://www.w3.org/XML/1998/namespace" />
    <xsd:element name="root" msdata:IsDataSet="true">
      <xsd:complexType>
        <xsd:choice maxOccurs="unbounded">
          <xsd:element name="metadata">
            <xsd:complexType>
              <xsd:sequence>
                <xsd:element name="value" type="xsd:string" minOccurs="0" />
              </xsd:sequence>
              <xsd:attribute name="name" use="required" type="xsd:string" />
              <xsd:attribute name="type" type="xsd:string" />
              <xsd:attribute name="mimetype" type="xsd:string" />
              <xsd:attribute ref="xml:space" />
            </xsd:complexType>
          </xsd:element>
          <xsd:element name="assembly">
            <xsd:complexType>
              <xsd:attribute name="alias" type="xsd:string" />
              <xsd:attribute name="name" type="xsd:string" />
            </xsd:complexType>
          </xsd:element>
          <xsd:element name="data">
            <xsd:complexType>
              <xsd:sequence>
                <xsd:element name="value" type="xsd:string" minOccurs="0" msdata:Ordinal="1" />
                <xsd:element name="comment" type="xsd:string" minOccurs="0" msdata:Ordinal="2" />
              </xsd:sequence>
              <xsd:attribute name="name" type="xsd:string" use="required" msdata:Ordinal="1" />
              <xsd:attribute name="type" type="xsd:string" msdata:Ordinal="3" />
              <xsd:attribute name="mimetype" type="xsd:string" msdata:Ordinal="4" />
              <xsd:attribute ref="xml:space" msdata:Ordinal="5" />
            </xsd:complexType>
          </xsd:element>
          <xsd:element name="resheader">
            <xsd:complexType>
              <xsd:sequence>
                <xsd:element name="value" type="xsd:string" minOccurs="0" msdata:Ordinal="1" />
              </xsd:sequence>
              <xsd:attribute name="name" type="xsd:string" use="required" />
            </xsd:complexType>
          </xsd:element>
        </xsd:choice>
      </xsd:complexType>
    </xsd:element>
  </xsd:schema>
  <resheader name="resmimetype">
    <value>text/microsoft-resx</value>
  </resheader>
  <resheader name="version">
    <value>2.0</value>
  </resheader>
  <resheader name="reader">
    <value>System.Resources.ResXResourceReader, System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089</value>
  </resheader>
  <resheader name="writer">
    <value>System.Resources.ResXResourceWriter, System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089</value>
  </resheader>
  <data name="App_Title" xml:space="preserve">
    <value>SceneTodo</value>
  </data>
  <data name="Common_OK" xml:space="preserve">
    <value>确定</value>
  </data>
  <data name="Common_Cancel" xml:space="preserve">
    <value>取消</value>
  </data>
  <data name="Common_Save" xml:space="preserve">
    <value>保存</value>
  </data>
  <data name="Common_Delete" xml:space="preserve">
    <value>删除</value>
  </data>
  <data name="Common_Edit" xml:space="preserve">
    <value>编辑</value>
  </data>
  <data name="Common_Add" xml:space="preserve">
    <value>添加</value>
  </data>
  <data name="Common_Search" xml:space="preserve">
    <value>搜索</value>
  </data>
  <data name="Common_Filter" xml:space="preserve">
    <value>筛选</value>
  </data>
  <data name="Common_Clear" xml:space="preserve">
    <value>清除</value>
  </data>
  <data name="Common_Close" xml:space="preserve">
    <value>关闭</value>
  </data>
  <data name="Common_Apply" xml:space="preserve">
    <value>应用</value>
  </data>
  <data name="Common_Reset" xml:space="preserve">
    <value>重置</value>
  </data>
  <data name="Menu_TodoListView" xml:space="preserve">
    <value>常规视图</value>
  </data>
  <data name="Menu_CalendarView" xml:space="preserve">
    <value>日历视图</value>
  </data>
  <data name="Menu_ScheduledTasks" xml:space="preserve">
    <value>计划任务</value>
  </data>
  <data name="Menu_History" xml:space="preserve">
    <value>历史记录</value>
  </data>
  <data name="Menu_Settings" xml:space="preserve">
    <value>设置</value>
  </data>
  <data name="Menu_Help" xml:space="preserve">
    <value>帮助</value>
  </data>
  <data name="Settings_Appearance" xml:space="preserve">
    <value>外观设置</value>
  </data>
  <data name="Settings_Language" xml:space="preserve">
    <value>语言设置</value>
  </data>
  <data name="Settings_Theme_Old" xml:space="preserve">
    <value>主题设置 (旧版)</value>
  </data>
  <data name="Settings_Backup" xml:space="preserve">
    <value>备份管理</value>
  </data>
  <data name="Settings_ResetConfig" xml:space="preserve">
    <value>重置配置</value>
  </data>
  <data name="Settings_ResetData" xml:space="preserve">
    <value>重置数据</value>
  </data>
  <data name="Search_Placeholder" xml:space="preserve">
    <value>搜索待办... (Ctrl+F)</value>
  </data>
  <data name="Filter_Button" xml:space="preserve">
    <value>筛选</value>
  </data>
  <data name="AdvancedSearch_Button" xml:space="preserve">
    <value>高级搜索</value>
  </data>
  <data name="Message_SaveSuccess" xml:space="preserve">
    <value>保存成功</value>
  </data>
  <data name="Message_SaveFailed" xml:space="preserve">
    <value>保存失败</value>
  </data>
  <data name="Message_Info" xml:space="preserve">
    <value>提示</value>
  </data>
  <data name="Message_Error" xml:space="preserve">
    <value>错误</value>
  </data>
  <data name="Message_LanguageSaved" xml:space="preserve">
    <value>语言设置已保存。重启应用后生效。</value>
  </data>
  <data name="Message_SettingsResetSuccess" xml:space="preserve">
    <value>设置已重置为默认值</value>
  </data>
  <data name="Message_OpenSettingsFailed" xml:space="preserve">
    <value>无法打开设置窗口</value>
  </data>
  <data name="Message_OpenLanguageSettingsFailed" xml:space="preserve">
    <value>无法打开语言设置</value>
  </data>
</root>
'@

# 英文资源文件模板
$englishResxTemplate = @'
<?xml version="1.0" encoding="utf-8"?>
<root>
  <xsd:schema id="root" xmlns="" xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:msdata="urn:schemas-microsoft-com:xml-msdata">
    <xsd:import namespace="http://www.w3.org/XML/1998/namespace" />
    <xsd:element name="root" msdata:IsDataSet="true">
      <xsd:complexType>
        <xsd:choice maxOccurs="unbounded">
          <xsd:element name="metadata">
            <xsd:complexType>
              <xsd:sequence>
                <xsd:element name="value" type="xsd:string" minOccurs="0" />
              </xsd:sequence>
              <xsd:attribute name="name" use="required" type="xsd:string" />
              <xsd:attribute name="type" type="xsd:string" />
              <xsd:attribute name="mimetype" type="xsd:string" />
              <xsd:attribute ref="xml:space" />
            </xsd:complexType>
          </xsd:element>
          <xsd:element name="assembly">
            <xsd:complexType>
              <xsd:attribute name="alias" type="xsd:string" />
              <xsd:attribute name="name" type="xsd:string" />
            </xsd:complexType>
          </xsd:element>
          <xsd:element name="data">
            <xsd:complexType>
              <xsd:sequence>
                <xsd:element name="value" type="xsd:string" minOccurs="0" msdata:Ordinal="1" />
                <xsd:element name="comment" type="xsd:string" minOccurs="0" msdata:Ordinal="2" />
              </xsd:sequence>
              <xsd:attribute name="name" type="xsd:string" use="required" msdata:Ordinal="1" />
              <xsd:attribute name="type" type="xsd:string" msdata:Ordinal="3" />
              <xsd:attribute name="mimetype" type="xsd:string" msdata:Ordinal="4" />
              <xsd:attribute ref="xml:space" msdata:Ordinal="5" />
            </xsd:complexType>
          </xsd:element>
          <xsd:element name="resheader">
            <xsd:complexType>
              <xsd:sequence>
                <xsd:element name="value" type="xsd:string" minOccurs="0" msdata:Ordinal="1" />
              </xsd:sequence>
              <xsd:attribute name="name" type="xsd:string" use="required" />
            </xsd:complexType>
          </xsd:element>
        </xsd:choice>
      </xsd:complexType>
    </xsd:element>
  </xsd:schema>
  <resheader name="resmimetype">
    <value>text/microsoft-resx</value>
  </resheader>
  <resheader name="version">
    <value>2.0</value>
  </resheader>
  <resheader name="reader">
    <value>System.Resources.ResXResourceReader, System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089</value>
  </resheader>
  <resheader name="writer">
    <value>System.Resources.ResXResourceWriter, System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089</value>
  </resheader>
  <data name="App_Title" xml:space="preserve">
    <value>SceneTodo</value>
  </data>
  <data name="Common_OK" xml:space="preserve">
    <value>OK</value>
  </data>
  <data name="Common_Cancel" xml:space="preserve">
    <value>Cancel</value>
  </data>
  <data name="Common_Save" xml:space="preserve">
    <value>Save</value>
  </data>
  <data name="Common_Delete" xml:space="preserve">
    <value>Delete</value>
  </data>
  <data name="Settings_Language" xml:space="preserve">
    <value>Language Settings</value>
  </data>
  <data name="Message_SaveSuccess" xml:space="preserve">
    <value>Saved Successfully</value>
  </data>
  <data name="Message_SaveFailed" xml:space="preserve">
    <value>Failed to Save</value>
  </data>
  <data name="Message_Info" xml:space="preserve">
    <value>Information</value>
  </data>
  <data name="Message_Error" xml:space="preserve">
    <value>Error</value>
  </data>
</root>
'@

# 写入文件（UTF-8 BOM）
$utf8WithBom = New-Object System.Text.UTF8Encoding $true
[System.IO.File]::WriteAllText("$resourceDir\Strings.resx", $chineseResxTemplate, $utf8WithBom)
[System.IO.File]::WriteAllText("$resourceDir\Strings.en.resx", $englishResxTemplate, $utf8WithBom)

Write-Host "Resource files created successfully!" -ForegroundColor Green
Write-Host "  - Resources\Strings.resx (Chinese)" -ForegroundColor Cyan
Write-Host "  - Resources\Strings.en.resx (English)" -ForegroundColor Cyan
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Yellow
Write-Host "1. Open the project in Visual Studio"
Write-Host "2. Open each resx file and set 'Access Modifier' to 'Public'"
Write-Host "3. Add more resource strings as needed"
Write-Host "4. Build the project"
