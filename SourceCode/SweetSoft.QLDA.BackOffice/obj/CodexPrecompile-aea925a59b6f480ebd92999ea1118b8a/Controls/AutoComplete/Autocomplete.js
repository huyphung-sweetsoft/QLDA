var UCAutocomplete = new Object();
UCAutocomplete.EmptySource = [];
$(document).ready(function () {
    $.ui.autocomplete.prototype._resizeMenu = function () {
        var ul = this.menu.element;
        ul.outerWidth(this.element.outerWidth());
    }
})

UCAutocomplete.SetValue = function (selector, item) {
    var shortItem = new Object();
    shortItem.Value = '';
    shortItem.Data = '';
    if (item !== null && typeof (item) === 'object') {
        if (item.Value !== undefined)
            shortItem.Value = item.Value.toString();
        if (item.Data !== undefined)
            shortItem.Data = item.Data.toString();
    }
    var items = [];
    items.push(shortItem);
    UCAutocomplete.SetValues(selector, items);
}
UCAutocomplete.SetValues = function (selector, items) {
    var elm;
    if (typeof (selector) === 'string')
        elm = $(selector);
    else
        elm = selector;
    //------------------
    var liveText = elm.find('input.live-text');
    var multipleLiveText = elm.find('.multiple-live-text');
    var hdfValue = $('#' + elm.attr('data-hdfvalue'));
    //SelectionMode
    var selectionMode = elm.attr('data-selectionmode');
    if (typeof (items) === 'object') {
        if (selectionMode === 'Single') {
            if (items[0] !== undefined && items[0].Value !== undefined)
                liveText.val(items[0].Value);
        }
        else if (selectionMode === 'Multiple') {
            multipleLiveText.empty();
            for (var i = 0; i < items.length; i++) {
                if (items[i].Value != "" && items[i].Data != "")
                    multipleLiveText.append(UCAutocomplete.MultipleItem(elm, items[i].Value, items[i].Data));
            }
        }
    }
    hdfValue.val(JSON.stringify(items));
    hdfValue.change();
}
UCAutocomplete.GetValue = function (selector) {
    var items = UCAutocomplete.GetValues(selector);
    if (items !== null && items !== undefined && items[0] !== undefined)
        return items[0];
    return null;
}
UCAutocomplete.GetValues = function (selector) {
    var elm;
    if (typeof (selector) === 'string')
        elm = $(selector);
    else
        elm = selector;
    //------------------
    var hdfValue = $('#' + elm.attr('data-hdfvalue'));
    if (hdfValue.val() === '' || hdfValue.val() === undefined)
        return null;
    return $.parseJSON(hdfValue.val());
}
UCAutocomplete.Cache = {
    data: {},
    remove: function (key) {
        delete UCAutocomplete.Cache.data[key];
    },
    exist: function (key) {
        return UCAutocomplete.Cache.data.hasOwnProperty(key) && UCAutocomplete.Cache.data[key] !== null;
    },
    get: function (key) {
        console.log('Getting in cache for:' + key);
        return UCAutocomplete.Cache.data[key];
    },
    set: function (key, cachedData, callback) {
        UCAutocomplete.Cache.remove(key);
        UCAutocomplete.Cache.data[key] = cachedData;
        if ($.isFunction(callback)) callback(cachedData);
    }
};
UCAutocomplete.MultipleItem = function (autocompleteElm, value, data) {
    var $multipleItem = $('<div class="multiple-item"><span>' + value + '</span></div>');
    var $remove = $('<a href="javascript:void(0);"><i class="fa fa-times" aria-hidden="true"></i></a>');
    $remove.click(function () {
        var multipleLiveText = autocompleteElm.find('.multiple-live-text');
        var hdfValue = $('#' + autocompleteElm.attr('data-hdfvalue'));

        var items = JSON.parse(hdfValue.val());
        items = items.filter(function (item) {
            return item.Data !== data;
        });
        //
        hdfValue.val(JSON.stringify(items));
        hdfValue.change();
        multipleLiveText.empty();
        for (var i = 0; i < items.length; i++) {
            if (items[i].Value != "" && items[i].Data != "")
                multipleLiveText.append(UCAutocomplete.MultipleItem(autocompleteElm, items[i].Value, items[i].Data));
        }
    })
    $multipleItem.prepend($remove);
    return $multipleItem;
}
UCAutocomplete._initAutocompleteElement = function (stringSelector) {
    var elm = $(stringSelector);
    if (elm.hasClass('autocomplete'))
        return;
    var disabled = elm.attr("data-enabled") == "False" ? "disabled = 'disabled'" : "";
    elm.addClass('btn-group bootstrap-select form-control dropup');
    elm.html(`<div class="multiple-live-text"></div>\
                <button type="button" class="btn dropdown-toggle btn-white ignore">\
                    <input class="live-text" tabindex="-1" aria-autocomplete="both" aria-haspopup="false" autofill="new-password" autocapitalize="off" autocomplete="new-password" autocorrect="off" autofocus="off" ${disabled}/>\
                    <span class="caret"></span>\
                </button>` + elm.html());
    //Source
    var source = [];
    var dataSource = elm.attr('data-source');
    if (dataSource !== undefined && dataSource != '')
        //source = eval(String.format('{0}("#{1}")', dataSource, elm.data('hdfvalue')));
        source = eval(dataSource);
    //MaxResult
    var maxResult;
    var dataMaxResult = elm.attr('data-maxresult');
    if (dataMaxResult !== undefined && dataMaxResult != '')
        maxResult = parseInt(dataMaxResult);
    //Lang
    var lang;
    var dataLang = elm.attr('data-lang');
    if (dataLang !== undefined && dataLang != '')
        //lang = parseInt(dataLang);
        lang = dataLang;
    //BottomSource
    var bottomSource = [];
    var dataBottomSource = elm.attr('data-sourcebottom');
    if (dataBottomSource !== undefined && dataBottomSource != '')
        bottomSource = eval(dataBottomSource);
    //TopSource
    var topSource = [];
    var dataTopSource = elm.attr('data-sourcetop');
    if (dataTopSource !== undefined && dataTopSource != '')
        topSource = eval(dataTopSource);
    //EmptyData
    var emptyData = elm.attr('data-emptydata');

    _autocompleteGuestType(emptyData, elm, source, maxResult, bottomSource, topSource, lang);
    function _autocompleteGuestType(emptyData, elm, source, maxResult, bottomSource, topSource, lang) {
        if (typeof (elm) == 'undefined' || !elm)
            return;
        var liveText = elm.find('input.live-text');
        var multipleLiveText = elm.find('.multiple-live-text');
        var hdfValue = $('#' + elm.attr('data-hdfvalue'));
        var button = elm.find('button');
        var appendTo = button;
        //PlaceHolder
        var placeholder = elm.attr('data-placeholder');
        if (placeholder !== undefined && placeholder != '')
            liveText.attr('placeholder', placeholder);
        //Style
        var style = elm.attr('data-style');
        if (style !== undefined)
            button.addClass(style);
        //Event
        var onchangeEvent = null;
        if (typeof (elm.attr('data-onchange')) != 'undefined' && elm.attr('data-onchange') != ''
            && typeof window[elm.attr('data-onchange')] === 'function') {
            onchangeEvent = eval(elm.attr('data-onchange'))
        }
        //SelectionMode
        var selectionMode = elm.attr('data-selectionmode');
        //if (selectionMode === 'Single') {
        liveText.attr('data-valid-type', 'inline');
        liveText.attr('data-validation-selector', elm.attr('data-hdfvalue'));
        liveText.attr('id', 'live-text' + elm.attr('data-hdfvalue'));
        //}
        //ValidationGroup
        var validationGroup = elm.attr('data-validationgroup');
        if (validationGroup !== undefined && validationGroup !== '') {
            if (selectionMode === 'Single') {
                liveText.addClass('vlg-' + validationGroup);
                liveText.attr('data-validation-engine', 'validate[required]');
            }
            else if (selectionMode === 'Multiple') {

            }
        }
        //ValidationGroup
        var required = elm.attr('data-required');
        if (selectionMode === 'Single' && required === 'true') {
            liveText.addClass('validate[required]');
        }
        //Parent
        var parentClass = elm.attr('data-parentClass');
        if (parentClass === undefined || parentClass === '')
            parentClass = 'form-group';

        _getSource(emptyData, source, maxResult, lang, bottomSource, topSource, function (emptyData, source, maxResult, lang, bottomSource, topSource) {
            var sp = liveText.on("keydown", function (event) {
                if (event.keyCode === $.ui.keyCode.TAB) {
                    event.preventDefault();
                }
            }).autocomplete({
                messages: {
                    noResults: '',
                    results: function () { }
                },
                focus: function (event, ui) {
                    if (ui == undefined || ui.item == undefined || ui.item.Data == undefined)
                        return;
                    var ul = event.currentTarget || event.delegateTarget;
                    //$(ul).find('li.ui-menu-item').removeClass('ui-cu-state-focus');
                    $(ul).find('li.ui-menu-item').removeClass('ui-state-focus');
                    var li = $(ul).find('li.ui-menu-item[data-id="' + ui.item.Data + '"]');
                    //li.addClass('ui-cu-state-focus');
                    li.addClass('ui-state-focus');
                },
                change: function (event, ui) {
                    if (ui.item == null) {
                        //liveText.val('');
                        hdfValue.val('');
                        hdfValue.change();

                        if (typeof (onchangeEvent) === 'function')
                            onchangeEvent(event, ui);
                    }
                },
                search: function (event, ui) {
                    $('.ui-autocomplete:visible').closest('.tab-pane.js-set-height').css('overflow', 'auto');
                    $('.ui-autocomplete:visible').removeClass('ui-autocomplete-open');
                    liveText.attr('data-lasttext', liveText.val());
                },
                select: function (event, ui) {
                    event.stopPropagation();
                    if (ui == undefined || ui.item == undefined || ui.item.Data == undefined)
                        return;
                    if (selectionMode === 'Single') {
                        liveText.val(ui.item.value);
                        var shortItemInArray = [];
                    }
                    else if (selectionMode === 'Multiple') {
                        liveText.val('');
                        var shortItemInArray;
                        if (hdfValue.val() === '' || hdfValue.val() === '[]') {
                            shortItemInArray = [];
                        }
                        else {
                            shortItemInArray = JSON.parse(hdfValue.val());
                        }
                    }

                    var shortItem = new Object();
                    shortItem.Value = ui.item.Value;
                    shortItem.Data = ui.item.Data;
                    shortItem.OtherData = ui.item.OtherData;

                    var containsObject = function (obj, list) {
                        for (var i = 0; i < list.length; i++) {
                            if (obj(list[i])) return true;
                        }
                        return false;
                    };
                    var pushIfNotExist = function (element, list, obj) {
                        if (!containsObject(obj, list)) {
                            list.push(element);
                        }
                    };
                    pushIfNotExist(shortItem, shortItemInArray, function (item) {
                        return item.Data === shortItem.Data;
                    })

                    hdfValue.val(JSON.stringify(shortItemInArray));
                    hdfValue.change();
                    hdfValue.next('.el-error').remove();

                    if (selectionMode === 'Multiple') {
                        multipleLiveText.empty();
                        for (var i = 0; i < shortItemInArray.length; i++) {
                            if (shortItemInArray[i].Value != "" && shortItemInArray[i].Data != "")
                                multipleLiveText.append(UCAutocomplete.MultipleItem(elm, shortItemInArray[i].Value, shortItemInArray[i].Data));
                        }
                    }

                    $('.ui-autocomplete.ui-autocomplete-open').closest('.tab-pane.js-set-height').css('overflow', 'auto');
                    $('.ui-autocomplete.ui-autocomplete-open').removeClass('ui-autocomplete-open');
                    liveText.removeClass("liveText-autocomplete-open");
                    elm.addClass('dropup');
                    autocompleteClosing = true;
                    //setTimeout(function () {
                    //    elm.parent().click();
                    //    elm.select();
                    //}, 100)
                    if (typeof (onchangeEvent) === 'function')
                        onchangeEvent(event, ui);
                    return false;
                },
                open: function (event, ui) {
                    var contentOuterHeight = $('.main-content .page-content').outerHeight();
                    var contentOffsetTop = $('.main-content .page-content').offset().top;
                    var liveTextOuterHeight = liveText.outerHeight();
                    var liveTextOffsetTop = liveText.offset().top;

                    var uiElement = $(this).data("ui-autocomplete").menu.element;

                    if ((liveTextOffsetTop + liveTextOuterHeight / 2) > (contentOffsetTop + contentOuterHeight / 2))
                        uiElement.css('top', (uiElement.outerHeight() + multipleLiveText.outerHeight() + 4) * -1);

                    elm.closest('.' + parentClass).addClass('au-open');
                    //--------------------------------------------------
                    var items = UCAutocomplete.GetValues(elm);
                    if (items != null && items != undefined && items.length > 0) {
                        items.forEach(function (item, i) {
                            var uiMenuItems = elm.find('.ui-autocomplete .ui-menu-item[data-id="' + item.Data + '"]');
                            uiMenuItems.addClass('selected');
                        })
                    }

                    elm.closest('.fieldset-box').addClass('open-autocomplete');
                },
                close: function () {
                    elm.closest('.' + parentClass).removeClass('au-open');
                    if (selectionMode === 'Single') {
                        setTimeout(function () {
                            var selectedItem = UCAutocomplete.GetValue(elm);
                            if (selectedItem === null && elm.attr('data-autoselect') === "1") {
                                var lastText = liveText.attr('data-lasttext');
                                UCAutocomplete.SetValue(elm, { Value: lastText, Data: lastText });
                                liveText.trigger('focusout');
                            }
                        }, 50)
                    }
                    else {
                        var selectedItem = UCAutocomplete.GetValue(elm);
                        if (selectedItem === null && elm.attr('data-autoselect') === "1") {
                            var lastText = liveText.attr('data-lasttext');
                            UCAutocomplete.SetValue(elm, { Value: lastText, Data: lastText });
                            //liveText.trigger('focusout');
                        }
                    }
                    elm.closest('.fieldset-box').removeClass('open-autocomplete');
                },
                appendTo: appendTo
            });
            sp.data("ui-autocomplete")._renderItem = function (ul, item) {
                if (item.Total !== undefined) {
                    var mes;
                    if (item.Total === 0) {
                        if (elm.attr('data-notfoundtext'))
                            mes = elm.attr('data-notfoundtext');
                        else
                            mes = UCAutocomplete.Language.NO_RESULTS;
                    }
                    else
                        mes = String.format(UCAutocomplete.Language.LIMITED_RESULTS, item.Total);
                    return $("<li style='pointer-events: none;'>").data("ui-autocomplete-item", item).append("<a><span style='width:30px;'>" + mes + '</span>' + "</a>").appendTo(ul);
                }
                else {
                    return $("<li data-id='" + item.Data + "'>")
                        .data("ui-autocomplete-item", item)
                        .append("<a><span style='width:30px;'>" + item.Label + '</span>' + "</a>")
                        .appendTo(ul);
                }
            };
            //OnInitCallbackSource
            var onInitCallbackSource;
            var dataOnInitCallbackSource = elm.attr('data-initcallback');
            if (dataOnInitCallbackSource !== undefined && dataOnInitCallbackSource != '')
                onInitCallbackSource = eval(dataOnInitCallbackSource);

            _bindSource(emptyData, source, bottomSource, topSource, maxResult, lang, onInitCallbackSource);
            function _bindSource(emptyData, source, bottomSource, topSource, maxResult, lang, onInitCallbackSource) {
                var rSource = source;
                var isFocus = liveText.attr("disabled") == "disabled" ? false : true;
                if (typeof (rSource) === 'object') {
                    if (bottomSource !== undefined)
                        rSource = rSource.concat(bottomSource);
                    if (topSource !== undefined)
                        rSource = topSource.concat(rSource);
                    liveText.autocomplete("option", "autoFocus", isFocus);

                    var processResult = function (emptyData, rSource, term, response) {
                        if (emptyData === "1" && rSource.length === 0 && term != '') {
                            rSource = [
                                { Label: term, Value: term, Data: term },
                            ];
                        }
                        rSource.forEach(function (o) {
                            o.label = o.Label;
                            o.value = o.Value;
                        });
                        response(rSource);

                        if (typeof (onInitCallbackSource) === 'function')
                            onInitCallbackSource(rSource);
                    };
                    liveText.autocomplete("option", "source", function (request, response) {
                        processResult(emptyData, $.ui.autocomplete.filter(rSource, request.term), request.term, response);
                    });
                } else if (typeof (rSource) === 'string') {
                    var processResult = function (emptyData, result, response) {
                        //response($.map($.parseJSON(result.d), function (item) {
                        //    return item;
                        //}));
                        //response($.parseJSON(result.d));
                        var resultTotal = [], spTotal = new Object;
                        spTotal.Total = $.parseJSON(result.d).Total;
                        resultTotal.push(spTotal);
                        if (spTotal.Total === 0) {
                            if (emptyData === "1") {
                                liveText.autocomplete("option", "autoFocus", isFocus);
                                var emptyData = liveText.val();
                                var listItem = [
                                    { Label: emptyData, Value: emptyData, Data: emptyData, label: emptyData, value: emptyData },
                                ];
                                response(listItem);
                                if (typeof (onInitCallbackSource) === 'function')
                                    onInitCallbackSource(listItem);
                            }
                            else {
                                liveText.autocomplete("option", "autoFocus", false);
                                response(resultTotal);
                            }
                        }
                        else if (spTotal.Total > maxResult) {
                            response(resultTotal);
                        }
                        else {
                            var listItem = $.parseJSON(result.d)?.ListAutocompleteItem;
                            if (bottomSource !== undefined)
                                listItem = listItem.concat(bottomSource);
                            if (topSource !== undefined)
                                listItem = topSource.concat(listItem);
                            listItem.forEach(function (o) {
                                o.label = o.Label;
                                o.value = o.Value;
                            });
                            response(listItem);
                            if (typeof (onInitCallbackSource) === 'function')
                                onInitCallbackSource(listItem);
                        }
                    };

                    liveText.autocomplete("option", "source", function (request, response) {
                        $.ajax({
                            type: "POST",
                            contentType: "application/json; charset=utf-8",
                            url: source,
                            data: JSON.stringify({ keyword: request.term, maxResult: maxResult, lang: lang }),
                            dataType: "json",
                            async: true,
                            cache: true,
                            beforeSend: function () {
                                var cacheKey = String.format('{0}{1}{2}{3}', source, request.term, maxResult, lang);
                                if (UCAutocomplete.Cache.exist(cacheKey)) {
                                    processResult(emptyData, UCAutocomplete.Cache.get(cacheKey), response);
                                    return false;
                                }
                                return true;
                            },
                            success: function (result) {
                                var cacheKey = String.format('{0}{1}{2}{3}', source, request.term, maxResult, lang);
                                UCAutocomplete.Cache.set(cacheKey, result);
                                processResult(emptyData, result, response);
                            }
                        });
                    });
                }
                else if (typeof (rSource) === 'function') {
                    var typeSource = rSource(elm, liveText.val());
                    if (typeof (typeSource) === 'string') {
                        var processResult = function (emptyData, result, response) {
                            var resultTotal = [], spTotal = new Object;
                            spTotal.Total = $.parseJSON(result.d).Total;
                            resultTotal.push(spTotal);
                            if (spTotal.Total === 0) {
                                if (emptyData === "1") {
                                    liveText.autocomplete("option", "autoFocus", isFocus);
                                    var emptyData = liveText.val();
                                    var listItem = [
                                        { Label: emptyData, Value: emptyData, Data: emptyData, label: emptyData, value: emptyData },
                                    ];
                                    response(listItem);
                                    if (typeof (onInitCallbackSource) === 'function')
                                        onInitCallbackSource(listItem);
                                }
                                else {
                                    liveText.autocomplete("option", "autoFocus", isFocus);
                                    response(resultTotal);
                                }
                            }
                            else if (spTotal.Total > maxResult) {
                                response(resultTotal);
                            }
                            else {
                                var listItem = $.parseJSON(result.d)?.ListAutocompleteItem;
                                if (bottomSource !== undefined)
                                    listItem = listItem.concat(bottomSource);
                                if (topSource !== undefined)
                                    listItem = topSource.concat(listItem);
                                listItem.forEach(function (o) {
                                    o.label = o.Label;
                                    o.value = o.Value;
                                });
                                response(listItem);
                                if (typeof (onInitCallbackSource) === 'function')
                                    onInitCallbackSource(listItem);
                            }
                        }
                        liveText.autocomplete("option", "source", function (request, response) {
                            $.ajax({
                                type: "POST",
                                contentType: "application/json; charset=utf-8",
                                url: rSource(elm, liveText.val()),
                                data: JSON.stringify({ keyword: request.term, maxResult: maxResult, lang: lang }),
                                dataType: "json",
                                async: true,
                                cache: true,
                                beforeSend: function () {
                                    var cacheKey = String.format('{0}{1}{2}{3}', rSource(elm, liveText.val()), request.term, maxResult, lang);
                                    if (UCAutocomplete.Cache.exist(cacheKey)) {
                                        processResult(emptyData, UCAutocomplete.Cache.get(cacheKey), response);
                                        return false;
                                    }
                                    return true;
                                },
                                success: function (result) {
                                    var cacheKey = String.format('{0}{1}{2}{3}', rSource(elm, liveText.val()), request.term, maxResult, lang);
                                    UCAutocomplete.Cache.set(cacheKey, result);
                                    processResult(emptyData, result, response);
                                }
                            });
                        });
                    } else if (typeof (typeSource) === 'object') {
                        liveText.autocomplete("option", "source", function (request, response) {
                            var typeSource = rSource(elm, liveText.val());
                            typeSource.forEach(function (o) {
                                o.label = o.Label;
                                o.value = o.Value;
                            });
                            response(typeSource);
                        });
                    }
                }
            }

            liveText.autocomplete("option", "minLength", 0);

            liveText.on("autocompleteopen", function (event, ui) {
                $('.ui-autocomplete:visible').closest('.tab-pane.js-set-height').css('overflow', '');
                $('.ui-autocomplete:visible').addClass('ui-autocomplete-open');
                liveText.addClass("liveText-autocomplete-open");
                elm.removeClass('dropup');
            });
            var autocompleteClosing = false;
            liveText.on("autocompleteclose", function (event, ui) {
                $('.ui-autocomplete.ui-autocomplete-open').closest('.tab-pane.js-set-height').css('overflow', 'auto');
                $('.ui-autocomplete.ui-autocomplete-open').removeClass('ui-autocomplete-open');
                liveText.removeClass("liveText-autocomplete-open");
                elm.addClass('dropup');
                autocompleteClosing = true;
                setTimeout(function () { autocompleteClosing = false; }, 100);
            });

            function _clickSearch() {
                if (!liveText.hasClass("liveText-autocomplete-open") && !autocompleteClosing) {
                    liveText.autocomplete("search", "");
                    liveText.select();
                }
            }
            button.bind('click', function (event) {
                _clickSearch();
            });
            multipleLiveText.bind('click', function (event) {
                _clickSearch();
            });

            //Set default live text
            if (hdfValue.val() !== '') {
                var jValue;
                try {
                    jValue = $.parseJSON(hdfValue.val());
                } catch (e) {
                    jValue = [];
                }

                if (typeof (jValue) === 'object') {
                    if (selectionMode === 'Single') {
                        if (jValue[0] !== undefined && jValue[0].Value !== undefined)
                            liveText.val(jValue[0].Value);
                    }
                    else if (selectionMode === 'Multiple') {
                        multipleLiveText.empty();
                        for (var i = 0; i < jValue.length; i++) {
                            if (jValue[i].Value != "" && jValue[i].Data != "")
                                multipleLiveText.append(UCAutocomplete.MultipleItem(elm, jValue[i].Value, jValue[i].Data));
                        }
                    }
                }
            }

            elm.addClass('autocomplete');
        })
        function _getSource(emptyData, source, maxResult, lang, bottomSource, topSource, callback) {
            if (typeof (source) === 'function')
                callback(emptyData, source, maxResult, lang, bottomSource, topSource);
            else if (typeof (source) === 'object')
                callback(emptyData, source, maxResult, lang, bottomSource, topSource);
            else if (typeof (source) === 'string') {
                if (maxResult !== undefined)
                    callback(emptyData, source, maxResult, lang, bottomSource, topSource);
                else {
                    var processResult = function (emptyData, result, callback) {
                        var listItem = $.parseJSON(result.d)?.ListAutocompleteItem;
                        callback(emptyData, listItem, maxResult, lang, bottomSource, topSource);
                    }
                    $.ajax({
                        type: "POST",
                        contentType: "application/json; charset=utf-8",
                        url: source,
                        data: JSON.stringify({ keyword: '', maxResult: 999999, lang: lang }),
                        dataType: "json",
                        async: true,
                        cache: true,
                        beforeSend: function () {
                            var cacheKey = String.format('{0}{1}{2}{3}', source, request.term, maxResult, lang);
                            if (UCAutocomplete.Cache.exist(cacheKey)) {
                                processResult(emptyData, UCAutocomplete.Cache.get(cacheKey), callback);
                                return false;
                            }
                            return true;
                        },
                        success: function (result) {
                            var cacheKey = String.format('{0}{1}{2}{3}', source, request.term, maxResult, lang);
                            UCAutocomplete.Cache.set(cacheKey, result);
                            processResult(emptyData, result, callback);
                        }
                    });
                }
            }
        }
    }
}

$(document).ready(function () {
    InitAutocomplete();
    Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function () {
        InitAutocomplete();
    });
});

function InitAutocomplete() {
    $('[data-autocomplete="true"]').each(function (index, elm) {
        UCAutocomplete._initAutocompleteElement(elm);
    })
};