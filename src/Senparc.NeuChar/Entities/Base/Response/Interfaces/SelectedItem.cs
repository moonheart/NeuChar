using System.Collections.Generic;

namespace Senparc.NeuChar.Entities;

/// <summary>
/// 用户选择项
/// </summary>
public class SelectedItem
{
    /// <summary>
    /// 问题的key值
    /// </summary>
    public string QuestionKey { get; set; }

    /// <summary>
    /// 对应问题的选项列表
    /// </summary>
    public List<string> OptionIds { get; set; }
}