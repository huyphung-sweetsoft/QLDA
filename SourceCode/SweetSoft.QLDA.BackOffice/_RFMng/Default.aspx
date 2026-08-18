<%@ Page Language="C#" AutoEventWireup="True"
    CodeBehind="Default.aspx.cs" Inherits="SweetSoft.QLDA.BackOffice._RFMng.Default" %>

<!DOCTYPE html>
<html>

<head>
    <meta http-equiv="content-type" content="text/html; charset=utf-8" />
    <title>File Manager</title>
    <link rel="stylesheet" type="text/css" href="/_RFMng/styles/reset.css" />
    <link rel="stylesheet" type="text/css" href="/_RFMng/scripts/jquery.filetree/src/jQueryFileTree.css" />
    <link rel="stylesheet" type="text/css" href="/_RFMng/scripts/jquery.contextmenu/dist/jquery.contextMenu.min.css" />
    <link rel="stylesheet" type="text/css" href="/_RFMng/scripts/custom-scrollbar-plugin/jquery.mCustomScrollbar.min.css" />
    <link rel="stylesheet" type="text/css" href="/_RFMng/themes/default/styles/filemanager.css">

    <style type="text/css">
        .fm-container #loading-wrap {
            position: fixed;
            height: 100%;
            width: 100%;
            overflow: hidden;
            top: 0;
            left: 0;
            display: block;
            background: white url(./images/wait30trans.gif) no-repeat center center;
            z-index: 999;
        }

        .hidden, .d-none {
            display: none !important;
        }
    </style>

    <link href="/_RFMng/scripts/lightbox-evolution-1.8/theme/default/jquery.lightbox.css" rel="stylesheet" />
    <style type="text/css">
        .jquery-lightbox-button-close {
            top: 12px;
            right: 12px;
            z-index: 7001;
        }

        .jquery-lightbox-html iframe {
            height: 100% !important;
            width: 100% !important;
        }

        #ddlpaging {
            position: absolute;
            left: 10px;
            /*bottom: 16px;*/
        }

        .bgtran {
            background: transparent !important
        }
        .mCSB_inside>.mCSB_container{
            overflow: auto;
        }
        .fm-container #contents.list
        {
            overflow:scroll;
        }
    </style>
    <!-- CSS dynamically added using 'config.options.theme' defined in config file -->
    <script>
        var _FMConfig = {
            pluginPath: "/_RFMng"
        }
    </script>
</head>
<body>
    <div class="fm-container" id="mainfm">
        <script type="text/javascript">
            if (location.search.length === 0)
                document.getElementById('mainfm').className = 'fm-container bgtran';
        </script>
        <div id="loading-wrap" style="display: none">
            <!-- loading wrapper / removed when loaded -->
        </div>
        <div>
            <form id="uploader" method="post" runat="server">
                <h1></h1>
                <button id="level-up" name="level-up" type="button" value="LevelUp">&nbsp;</button>
                <button id="home" name="home" type="button" value="Home">&nbsp;</button>
                <input id="mode" name="mode" type="hidden" value="add" />
                <input id="currentpath" name="currentpath" type="hidden" />
                <div id="file-input-container">
                    <div id="alt-fileinput">
                        <input id="filepath" name="filepath" type="text" />
                        <button id="browse" name="browse" type="button" value="Browse"></button>
                    </div>
                    <input id="newfile" name="newfile" type="file" />
                </div>
                <button id="upload" name="upload" type="button" value="Upload" class="em"></button>
                <button id="newfolder" name="newfolder" type="button" value="New Folder" class="em"></button>
                <button id="grid" class="ON" type="button">&nbsp;</button>
                <button id="list" type="button">&nbsp;</button>
            </form>

            <div id="splitter">
                <div id="filetree"></div>
                <div id="fileinfo">
                    <h1></h1>
                </div>
            </div>

            <div id="footer">
                <form name="search" id="search" method="get">
                    <div>
                        <input type="text" value="" name="q" id="q" />
                        <a id="reset" href="#" class="q-reset"></a>
                        <span class="q-inactive"></span>
                    </div>
                </form>
                <div class="right">
                    <div id="folder-info">
                        <span id="items-counter"></span>- <span id="items-size"></span>
                    </div>
                    <div id="summary"></div>
                </div>
                <div style="clear: both"></div>
                <select id="ddlpaging" style="display: none">
                    <option value="10">10</option>
                    <option selected="selected" value="30">30</option>
                    <option value="50">50</option>
                    <option value="100">100</option>
                    <option value="150">150</option>
                    <option value="200">200</option>
                </select>
            </div>
            <script type="text/javascript" src="/_RFMng/scripts/jquery-1.11.3.min.js"></script>
            <script type="text/javascript" src="/_RFMng/scripts/jquery-ui/version.js"></script>
            <script type="text/javascript" src="/_RFMng/scripts/jquery-ui/ie.js"></script>
            <script type="text/javascript" src="/_RFMng/scripts/jquery-ui/data.js"></script>
            <script type="text/javascript" src="/_RFMng/scripts/jquery-ui/plugin.js"></script>
            <script type="text/javascript" src="/_RFMng/scripts/jquery-ui/safe-active-element.js"></script>
            <script type="text/javascript" src="/_RFMng/scripts/jquery-ui/safe-blur.js"></script>
            <script type="text/javascript" src="/_RFMng/scripts/jquery-ui/unique-id.js"></script>
            <script type="text/javascript" src="/_RFMng/scripts/jquery-ui/scroll-parent.js"></script>
            <script type="text/javascript" src="/_RFMng/scripts/jquery-ui/widget.js"></script>
            <script type="text/javascript" src="/_RFMng/scripts/jquery-ui/mouse.js"></script>
            <script type="text/javascript" src="/_RFMng/scripts/jquery-ui/position.js"></script>
            <script type="text/javascript" src="/_RFMng/scripts/jquery-ui/draggable.js"></script>
            <script type="text/javascript" src="/_RFMng/scripts/jquery-ui/droppable.js"></script>
            <script type="text/javascript" src="/_RFMng/scripts/jquery-browser.js"></script>
            <script type="text/javascript" src="/_RFMng/scripts/jquery.splitter/jquery.splitter-1.5.1.js"></script>
            <script type="text/javascript" src="/_RFMng/scripts/jquery.filetree/src/jQueryFileTree.js"></script>
            <script type="text/javascript" src="/_RFMng/scripts/jquery.contextmenu/dist/jquery.contextMenu.min.js"></script>
            <script type="text/javascript" src="/_RFMng/scripts/jquery.impromptu/dist/jquery-impromptu.min.js"></script>
            <script type="text/javascript" src="/_RFMng/scripts/TinySort/dist/tinysort.js"></script>
            <script type="text/javascript" src="/_RFMng/scripts/TinySort/dist/jquery.tinysort.js"></script>
            <script type="text/javascript" src="/_RFMng/scripts/javascript-templates/js/tmpl.min.js"></script>
            <script type="text/javascript" src="/_RFMng/scripts/jQuery-File-Upload/js/canvas-to-blob.min.js"></script>
            <script type="text/javascript" src="/_RFMng/scripts/jQuery-File-Upload/js/load-image.all.min.js"></script>
            <script type="text/javascript" src="/_RFMng/scripts/jQuery-File-Upload/js/jquery.iframe-transport.js"></script>
            <script type="text/javascript" src="/_RFMng/scripts/jQuery-File-Upload/js/jquery.fileupload.js"></script>
            <script type="text/javascript" src="/_RFMng/scripts/jQuery-File-Upload/js/jquery.fileupload-process.js"></script>
            <script type="text/javascript" src="/_RFMng/scripts/jQuery-File-Upload/js/jquery.fileupload-image.js"></script>
            <script type="text/javascript" src="/_RFMng/scripts/jQuery-File-Upload/js/jquery.fileupload-validate.js"></script>
            <script type="text/javascript" src="/_RFMng/scripts/jquery.single_double_click.js"></script>
            <script src="/_RFMng/scripts/lightbox-evolution-1.8/js/jquery.lightbox.1.8.min.js"></script>
            <script type="text/javascript" src="/_RFMng/scripts/filemanager.js?v=202432210251418"></script>
            <script>
                $(document).ready(function () {
                    $('#list')[0].click();
                })
            </script>
        </div>
    </div>
</body>

</html>
