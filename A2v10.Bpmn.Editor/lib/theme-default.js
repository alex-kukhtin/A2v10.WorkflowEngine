ace.define("ace/theme/default.css", ["require", "exports", "module"],
    function (e, t, n) {
        n.exports =
        '.ace-default{background-color:#fff;color:#000}.ace-default .ace_gutter{background:#ebebeb;color:#333;overflow:hidden}.ace-default .ace_print-margin{width:1px;background:#e8e8e8}.ace-default .ace_identifier{color:#000}.ace-default .ace_keyword{color:#00f}.ace-default .ace_numeric{color:#000}.ace-default .ace_storage{color:#00f}.ace-default .ace_keyword.ace_operator,.ace-default .ace_lparen,.ace-default .ace_rparen,.ace-default .ace_punctuation{color:#000}.ace-default .ace_set.ace_statement{color:#00f;text-decoration:underline}.ace-default .ace_cursor{color:#000}.ace-default .ace_invisible{color:#bfbfbf}.ace-default .ace_constant.ace_buildin{color:#5848f6}.ace-default .ace_constant.ace_language{color:#00f}.ace-default .ace_constant.ace_library{color:#06960e}.ace-default .ace_invalid{background-color:#900;color:#fff}.ace-default .ace_class{color:#008080}.ace-default .ace_support.ace_other{color:#6d79de}.ace-default .ace_comment{color:#008000}.ace-default .ace_constant.ace_numeric{color:#000;font-weight:bold}.ace-default .ace_variable{color:#000}.ace-default .ace_heading{color:#0c07ff}.ace-default .ace_list{color:#b90690}.ace-default .ace_marker-layer .ace_selection{background:#b5d5ff}.ace-default .ace_marker-layer .ace_step{background:#fcff00}.ace-default .ace_marker-layer .ace_stack{background:#a4e565}.ace-default .ace_marker-layer .ace_bracket{margin:-1px 0 0 -1px;border:1px solid #c0c0c0}.ace-default .ace_marker-layer .ace_active-line{background:rgba(0,0,0,.07)}.ace-default .ace_gutter-active-line{background-color:#dcdcdc}.ace-default .ace_marker-layer .ace_selected-word{background:#fafaff;border:1px solid #c8c8fa}.ace-default .ace_meta.ace_tag{color:#00f}.ace-default .ace_string.ace_regex{color:#af1515}.ace-default .ace_string{color:#af1515}.ace-default .ace_entity.ace_other.ace_attribute-name{color:#994409}.ace-default .ace_indent-guide{background:url("data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAACCAYAAACZgbYnAAAAE0lEQVQImWP4////f4bLly//BwAmVgd1/w11/gAAAABJRU5ErkJggg==") right repeat-y}.ace-default .ace_indent-guide-active{background:url("data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAACCAYAAACZgbYnAAAACXBIWXMAAAsTAAALEwEAmpwYAAAAIGNIUk0AAHolAACAgwAA+f8AAIDpAAB1MAAA6mAAADqYAAAXb5JfxUYAAAAZSURBVHjaYvj///9/hivKyv8BAAAA//8DACLqBhbvk+/eAAAAAElFTkSuQmCC") right repeat-y}'
    }),
    ace.define("ace/theme/default",
        ["require", "exports", "module", "ace/theme/default.css", "ace/lib/dom"],
        function (e, t, n) {
            t.isDark = !1,
            t.cssClass = "ace-default",
            t.cssText = e("./default.css");
            var r = e("../lib/dom");
            r.importCssString(t.cssText, t.cssClass, !1)
        });
(function () {
    ace.require(["ace/theme/default"], function (m) {
        if (typeof module == "object" && typeof exports == "object" && module) {
            module.exports = m;
        }
    });
})();
