using System.Text.RegularExpressions;

namespace DHTMLX.Export.PDF
{
    public class TreeTimelineCategory
    {
        public bool Expanded { get; set; }
        public int Level { get; set; }
        public bool IsExpandable { get; set; }
        public string Text { get; set; }
        public TreeTimelineCategory(string text, int level, bool allowchild, bool expanded)
        {
            Level = level;
            Expanded = expanded;
            IsExpandable = allowchild;
            Text = text;
        }
        public TreeTimelineCategory() : this("", 0, false, false)
        {

        }
    }


    public class TreeTimelineParser
    {

        /*
         * Html tree timeline rows, e.g.:
        <div class=\"dhx_scell_level0\">
           <div class=\"dhx_scell_expand\">-</div><div class=\"dhx_scell_name\">Web Testing Dep.</div>
        </div>
        <div class=\"dhx_scell_level1\">
           <div class=\"dhx_scell_expand\"> </div><div class=\"dhx_scell_name\">Managers</div>
         </div>
        <div class=\"dhx_scell_level1\">
           <div class=\"dhx_scell_name\">Elizabeth Taylor</div>
        </div>"
        */
        public static bool IsTreeRow(string item)
        {
            var treeRowRegex = "^<div.*dhx_scell_level.*>.*(dhx_scell_expand|dhx_scell_name).*<\\/div>$";
            return (Regex.IsMatch(item.Trim(), treeRowRegex));
        }
        public TreeTimelineCategory Parse(string item)
        {
            var levelReg = "dhx_scell_level([0-9]+)";
            var isExpandable = "dhx_scell_expand";
            var state = "dhx_scell_expand.*?>(-| )<\\/div";
            var text = "dhx_scell_name.*?>(.+?)<\\/div";
            var expandable = Regex.IsMatch(item, isExpandable);
            var level = 0;
            int.TryParse(Regex.Match(item, levelReg).Groups[1].Value, out level);
            var expanded = Regex.Match(item, state).Groups[1].Value == "-";
            var value = Regex.Match(item, text).Groups[1].Value;
            return new TreeTimelineCategory(value, level, expandable, expanded);


        }
    }
}
