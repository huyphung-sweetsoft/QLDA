
if (typeof window.BootStrap_NET === 'undefined')
    window.BootStrap_NET = {};

BootStrap_NET.ExtraDropdown = {
    data: {
        color: ['has-info', 'has-danger', 'has-primary', 'has-default',
            'has-warning', 'has-error', 'has-success',
            'select2-info', 'select2-danger', 'select2-primary', 'select2-default',
            'select2-warning', 'select2-error', 'select2-success'],
        laterInit: [],
        cacheObjectFunc: undefined,
        cacheData: []
    },
    cacheObject: {
        initCacheObjectFunc: function () {

            function localCache(id, maxCacheNum, cacheOnLoad
                , cacheKeyWordMinLength, autoCache) {

                this._id = id || '';
                this._data = {};
                this._count = 0;

                this._maxCache = -1;
                if (typeof maxCacheNum === 'number')
                    this._maxCache = maxCacheNum;

                this._cacheOnLoad = false;
                if (typeof cacheOnLoad === 'boolean')
                    this._cacheOnLoad = cacheOnLoad;

                this._autoCache = true;
                if (typeof autoCache === 'boolean')
                    this._autoCache = autoCache;

                this._cacheKeyWordMinLength = -1;
                if (typeof cacheKeyWordMinLength === 'number')
                    this._cacheKeyWordMinLength = cacheKeyWordMinLength;
            };

            localCache.prototype.remove = function (url) {
                if (this.exist(url) === true) {
                    delete this._data[url];
                    this._count--;
                }
            };
            localCache.prototype.exist = function (url) {
                return this._data.hasOwnProperty(url) && this._data[url] !== null;
            };
            localCache.prototype.get = function (url) {
                console.log('Getting in cache for url' + url);
                return this._data[url];
            };
            localCache.prototype.set = function (url, keyword, page, cachedData, callback) {
                if (keyword.length < this._cacheKeyWordMinLength) {
                    console.log('Can not save data because of keyword must equal or greater than ' +
                        this._cacheKeyWordMinLength + '.');
                }
                else {
                    var fullurl = url || '';
                    fullurl += '_' + keyword + '_' + page;
                    if (this.exist(fullurl))
                        this._data[fullurl] = cachedData;
                    else {
                        if ((this._maxCache === -1) || (this._count < this._maxCache)) {
                            this._count++;
                            this._data[fullurl] = cachedData;
                        }
                        else if (this._maxCache > 0)
                            console.log('Can not save data because of maximum cache reach.');
                    }
                }
                if ($.isFunction(callback))
                    callback(cachedData);
            };
            localCache.prototype.countItem = function () { return this._count; }

            BootStrap_NET.ExtraDropdown.data.cacheObjectFunc = localCache;
        },
        findCache: function (id) {
            if (typeof id === 'undefined' || id === null || id.length === 0 ||
                BootStrap_NET.ExtraDropdown.data.cacheData.length === 0)
                return undefined;
            var found = undefined;
            $.each(BootStrap_NET.ExtraDropdown.data.cacheData, function (i, o) {
                if (o._id === id) {
                    found = o;
                    return false;
                }
            });
            return found;
        },
        addCacheForElement: function (id, numPage, cacheOnLoad, cacheKeyWordMinLength, autoCache) {
            if (typeof id === 'undefined' || id === null || id.length === 0) {
                console.log('Can not create cache for element does not have id.');
                return null;
            }

            var indx = -1;
            $.each(BootStrap_NET.ExtraDropdown.data.cacheData, function (i, o) {
                if (o._id === id) {
                    indx = i;
                    return false;
                }
            });

            var obj = new BootStrap_NET.ExtraDropdown.data.cacheObjectFunc(id, numPage,
                cacheOnLoad, cacheKeyWordMinLength, autoCache);
            if (indx !== -1)
                BootStrap_NET.ExtraDropdown.data.cacheData[indx] = obj;
            else
                BootStrap_NET.ExtraDropdown.data.cacheData.push(obj);
            return obj;
        }
    },
    commonFunction: {
        isIE: function () {
            var myNav = navigator.userAgent.toLowerCase();
            return (myNav.indexOf('msie') != -1) ? parseInt(myNav.split('msie')[1]) : false;
        },
        getIEVersion: function () {
            var rv = -1;
            if (navigator.appName == 'Microsoft Internet Explorer') {
                var ua = navigator.userAgent;
                var re = new RegExp("MSIE ([0-9]{1,}[\.0-9]{0,})");
                if (re.exec(ua) != null)
                    rv = parseFloat(RegExp.$1);
            }
            else if (navigator.appName == 'Netscape') {
                var ua = navigator.userAgent;
                var re = new RegExp("Trident/.*rv:([0-9]{1,}[\.0-9]{0,})");
                if (re.exec(ua) != null)
                    rv = parseFloat(RegExp.$1);
            }
            return rv;
        },
        getDataQuery: function (name, ss) {
            if (typeof ss !== 'undefined' && ss.length > 0) {
                if (ss.indexOf('?') !== 0)
                    ss = '?' + ss;
            }
            name = name.replace(/[\[]/, "\\[").replace(/[\]]/, "\\]");
            var regex = new RegExp("[\\?&]" + name + "=([^&#]*)", 'i'),
                results = regex.exec(ss || location.search);
            return results === null ? "" : decodeURIComponent(results[1].replace(/\+/g, " "));
        },
        findEventHandlers: function (eventType, jqSelector) {
            var results = [];
            var jQ = jQuery;// to avoid conflict between others frameworks like Mootools

            var arrayIntersection = function (array1, array2) {
                return jQ(array1).filter(function (index, element) {
                    return jQ.inArray(element, jQ(array2)) !== -1;
                });
            };

            var haveCommonElements = function (array1, array2) {
                return arrayIntersection(array1, array2).length !== 0;
            };


            var addEventHandlerInfo = function (element, event, $elementsCovered) {
                var extendedEvent = event;
                if ($elementsCovered !== void 0 && $elementsCovered !== null) {
                    jQ.extend(extendedEvent, { targets: $elementsCovered.toArray() });
                }
                var eventInfo;
                var eventsInfo = jQ.grep(results, function (evInfo, index) {
                    return element === evInfo.element;
                });

                if (eventsInfo.length === 0) {
                    eventInfo = {
                        element: element,
                        events: [extendedEvent]
                    };
                    results.push(eventInfo);
                } else {
                    eventInfo = eventsInfo[0];
                    eventInfo.events.push(extendedEvent);
                }
            };


            var $elementsToWatch = jQ(jqSelector);
            if (jqSelector === "*")//* does not include document and we might be interested in handlers registered there
                $elementsToWatch = $elementsToWatch.add(document);
            var $allElements = jQ("*").add(document);

            jQ.each($allElements, function (elementIndex, element) {
                var allElementEvents = jQ._data(element, "events");
                if (allElementEvents !== void 0 && allElementEvents[eventType] !== void 0) {
                    var eventContainer = allElementEvents[eventType];
                    jQ.each(eventContainer, function (eventIndex, event) {
                        var isDelegateEvent = event.selector !== void 0 && event.selector !== null;
                        var $elementsCovered;
                        if (isDelegateEvent) {
                            $elementsCovered = jQ(event.selector, element); //only look at children of the element, since those are the only ones the handler covers
                        } else {
                            $elementsCovered = jQ(element); //just itself
                        }
                        if (haveCommonElements($elementsCovered, $elementsToWatch)) {
                            addEventHandlerInfo(element, event, $elementsCovered);
                        }
                    });
                }
            });

            return results;
        }
    },
    mainFunction: {
        init: function (sender, args) {
            if (typeof BootStrap_NET.ExtraDropdown.data.cacheObjectFunc === 'undefined') {
                BootStrap_NET.ExtraDropdown.cacheObject.initCacheObjectFunc();
                jQuery.fn.select2.amd.require(['select2/core'], function (Core) {
                    //console.log(Core.selection);
                    Core.prototype.clearData = function () {
                        if (typeof this.$element !== 'undefined')
                            this.$element.val('').trigger('change');
                    };
                });
                jQuery.fn.select2.amd.require([
               'select2/utils', 'select2/data/ajax', 'select2/data/minimumInputLength'
                ], function (Utils, AjaxAdapter, MinimumInputLength) {
                    AjaxAdapter.prototype.query = function (params, callback) {

                        var matches = [];
                        var self = this;

                        if (this._request != null) {
                            // JSONP requests cannot always be aborted
                            if ($.isFunction(this._request.abort)) {
                                this._request.abort();
                            }

                            this._request = null;
                        }


                        var cacheObject = undefined;
                        var id = this.$element.attr('id');
                        if (typeof id !== 'undefined' && id.length > 0)
                            cacheObject = BootStrap_NET.ExtraDropdown.cacheObject.findCache(id);

                        var options = $.extend({
                            type: 'GET'
                        }, this.ajaxOptions);

                        var oldBS = options.beforeSend;
                        if ($.isFunction(oldBS) === true) {
                            //console.log('old options.beforeSend : ', old);
                            options.beforeSend = function (jqXHR, settings) {
                                oldBS.call(this, jqXHR, settings, params, cacheObject);
                            }
                        }

                        var oldSC = options.success;
                        if ($.isFunction(oldSC) === true) {
                            //console.log('old options.beforeSend : ', old);
                            options.success = function (jqXHR, settings) {
                                oldSC.call(this, jqXHR, settings, params, cacheObject);
                            }
                        }


                        if (typeof options.url === 'function') {
                            options.url = options.url.call(this.$element, params);
                        }

                        if (typeof options.data === 'function') {
                            options.data = options.data.call(this.$element, params);
                        }

                        function request() {
                            var $request = options.transport(options, function (data) {
                                //console.log(options, data);

                                if (typeof cacheObject !== 'undefined' && cacheObject._autoCache === true) {
                                    var dataUrl = this.url;
                                    var page = params.page || 1;
                                    var ke = params.term || '';
                                    cacheObject.set(dataUrl, ke, page, [data, params], function () { });
                                }

                                var results = self.processResults(data, params);
                                if (self.options.get('debug') && window.console && console.error) {
                                    // Check to make sure that the response included a `results` key.
                                    if (!results || !results.results || !$.isArray(results.results)) {
                                        console.error(
                                          'Select2: The AJAX results did not return an array in the ' +
                                          '`results` key of the response.'
                                        );
                                    }
                                }
                                callback(results);
                            }, function () {
                                // TODO: Handle AJAX errors
                            });

                            self._request = $request;
                        }

                        if (this.ajaxOptions.delay && params.term !== '') {
                            if (this._queryTimeout) {
                                window.clearTimeout(this._queryTimeout);
                            }

                            this._queryTimeout = window.setTimeout(request, this.ajaxOptions.delay);
                        } else {
                            request();
                        }
                    };

                    BootStrap_NET.ExtraDropdown.mainFunction.mainInit(sender, args);
                });
            }
            else
                BootStrap_NET.ExtraDropdown.mainFunction.mainInit(sender, args);
        },
        mainInit: function (sender, args) {

            var $ddl = $("select.select2:not(.ignore-select2)");
            if ($ddl.length > 0) {
                BootStrap_NET.ExtraDropdown.data.laterInit = [];

                var data = undefined;
                $ddl.each(function (i, el) {
                    data = $(el).attr('data-initAfterLoad');
                    if (typeof data !== 'undefined' && data.length > 0
                        && $.trim(data).toLowerCase() === 'true') {
                        BootStrap_NET.ExtraDropdown.data.laterInit.push(el);
                    }
                    else
                        BootStrap_NET.ExtraDropdown.mainFunction.initForElement(el);
                });

                if (typeof args !== 'undefined') {

                    BootStrap_NET.ExtraDropdown.mainFunction.initWindowLoad();
                }
            }
        },
        initWindowLoad: function () {
            if ($.isArray(BootStrap_NET.ExtraDropdown.data.laterInit) === true) {
                $.each(BootStrap_NET.ExtraDropdown.data.laterInit, function (i, o) {
                    BootStrap_NET.ExtraDropdown.mainFunction.initForElement(o);
                });
                BootStrap_NET.ExtraDropdown.data.laterInit = [];
            }
        },
        countProperties: function (obj) {
            var count = "__count__", hasOwnProp = Object.prototype.hasOwnProperty;

            if (typeof obj[count] === "number" && !hasOwnProp.call(obj, count)) {
                return obj[count];
            }
            count = 0;
            for (var prop in obj) {
                if (hasOwnProp.call(obj, prop)) {
                    count++;
                }
            }
            return count;
        },
        clearSelect: function (o) {
            if (typeof o !== 'undefined')
                $(o).select2('clearData');
        },
        initForElement: function (o) {
            if (typeof o === 'undefined' || o === null)
                return;

            var opts = {};

            /*#region parse setting */

            var data = $(o).attr('data-inited');
            if (typeof data !== 'undefined' && data.length > 0) {
                return;
            }
            $(o).attr('data-inited', '1');

            /*#region prop*/

            data = $(o).attr('data-placeholder');
            if (typeof data !== 'undefined' && data.length > 0) {
                opts['placeholder'] = $.trim(data);
            }

            data = $(o).attr('data-dropdownParent');
            if (typeof data !== 'undefined' && data.length > 0) {
                var elem = $($.trim(data));
                if (elem.length > 0)
                    opts['dropdownParent'] = elem;
            }

            data = $(o).attr('data-jsonDataArray');
            if (typeof data !== 'undefined' && data.length > 0
                && typeof window[$.trim(data)] !== 'undefined'
                && $.isArray(window[$.trim(data)]) === true) {
                opts['data'] = jQuery.extend(true, [], window[$.trim(data)])
            }

            data = $(o).attr('data-maximumInputLength');
            if (typeof data !== 'undefined' && data.length > 0) {
                opts['maximumInputLength'] = parseInt($.trim(data)) || 0;
            }

            data = $(o).attr('data-minimumInputLength');
            if (typeof data !== 'undefined' && data.length > 0) {
                opts['minimumInputLength'] = parseInt($.trim(data)) || 0;
            }

            data = $(o).attr('data-maximumSelectionLength');
            if (typeof data !== 'undefined' && data.length > 0) {
                opts['maximumSelectionLength'] = parseInt($.trim(data)) || 0;
            }

            data = $(o).attr('data-minimumResultsForSearch');
            if (typeof data !== 'undefined' && data.length > 0) {
                if ($.trim(data) == 'Infinity')
                    opts['minimumResultsForSearch'] = 'Infinity';
                else
                    opts['minimumResultsForSearch'] = parseInt($.trim(data)) || 0;
            }

            data = $(o).attr('data-debug');
            if (typeof data !== 'undefined' && data.length > 0) {
                opts['debug'] = $.trim(data).toLowerCase() === 'true' ? true : false;
            }

            data = $(o).attr('data-selectOnClose');
            if (typeof data !== 'undefined' && data.length > 0) {
                opts['selectOnClose'] = $.trim(data).toLowerCase() === 'true' ? true : false;
            }

            data = $(o).attr('data-closeOnSelect');
            if (typeof data !== 'undefined' && data.length > 0) {
                opts['closeOnSelect'] = $.trim(data).toLowerCase() === 'true' ? true : false;
            }

            data = $(o).attr('data-dropdownAutoWidth');
            if (typeof data !== 'undefined' && data.length > 0) {
                opts['dropdownAutoWidth'] = $.trim(data).toLowerCase() === 'true' ? true : false;
                //-----------------------------
                opts['createTag'] = function (params) {
                    var term = $.trim(params.term);

                    if (term === '') {
                        return null;
                    }

                    // check whether the term matches an id
                    var search = $.grep(options, function (n, i) {
                        return (n.id === term || n.text === term); // check against id and text
                    });

                    // if a match is found replace the term with the options' text
                    if (search.length)
                        term = search[0].text;
                    else
                        return null; // didn't match id or text value so don't add it to selection

                    return {
                        id: term,
                        text: term,
                        value: true // add additional parameters
                    }
                }
            }

            data = $(o).attr('data-tags');
            if (typeof data !== 'undefined' && data.length > 0) {
                opts['tags'] = $.trim(data).toLowerCase() === 'true' ? true : false;
            }

            data = $(o).attr('data-allowClear');
            if (typeof data !== 'undefined' && data.length > 0) {
                opts['allowClear'] = $.trim(data).toLowerCase() === 'true' ? true : false;

                if (typeof opts['placeholder'] === 'undefined' || opts['placeholder'] === null) {
                    var opFirst = $(o).find('option[value=""]:first,option:not([value]):first');
                    if (opFirst.length > 0)
                        opts['placeholder'] = opFirst.text();
                    else
                        opts['placeholder'] = '';
                }
            }

            data = $(o).attr('data-dir');
            if (typeof data !== 'undefined' && data.length > 0) {
                opts['dir'] = $.trim(data);
            }

            data = $(o).attr('data-afterInitFunction');
            if (typeof data !== 'undefined' && data.length > 0
            && typeof window[$.trim(data)] === 'function') {
                opts['afterInitFunction'] = window[$.trim(data)];
            }

            data = $(o).attr('data-escapeMarkupFunction');
            if (typeof data !== 'undefined' && data.length > 0
            && typeof window[$.trim(data)] === 'function') {
                opts['escapeMarkup'] = window[$.trim(data)];
            }

            data = $(o).attr('data-containerCssClass');
            if (typeof data !== 'undefined' && data.length > 0) {
                opts['containerCssClass'] = $.trim(data);
            }

            data = $(o).attr('data-dropdownCssClass');
            if (typeof data !== 'undefined' && data.length > 0) {
                opts['dropdownCssClass'] = $.trim(data);
            }

            var hasCascading = false;

            data = $(o).attr('data-ajaxCascadingWith');
            if (typeof data !== 'undefined' && data.length > 0) {
                var elemCas = $($.trim(data));
                if (elemCas.length > 0) {
                    hasCascading = true;

                    opts['ajaxCascadingWith'] = elemCas;

                    elemCas.on('change', function () {
                        BootStrap_NET.ExtraDropdown.mainFunction.clearSelect(o);
                    });

                    elemCas.on('select2:unselect', function () {
                        BootStrap_NET.ExtraDropdown.mainFunction.clearSelect(o);
                    });
                }
            }

            data = $(o).attr('data-theme');
            if (typeof data !== 'undefined' && data.length > 0) {
                opts['theme'] = $.trim(data);
            }

            data = $(o).attr('data-language');
            if (typeof data !== 'undefined' && data.length > 0) {
                opts['language'] = $.trim(data);
            }

            data = $(o).attr('data-sorterFunction');
            if (typeof data !== 'undefined' && data.length > 0
            && typeof window[$.trim(data)] === 'function') {
                opts['sorter'] = window[$.trim(data)]();
            }

            data = $(o).attr('data-tokenSeparators');
            if (typeof data !== 'undefined' && data.length > 0) {
                opts['tokenSeparators'] = eval($.trim(data));
            }

            /*#endregion*/

            /*#region ajax*/

            var ajaxObj = undefined;
            data = $(o).attr('data-ajax-objectSetting');
            if (typeof data !== 'undefined' && data.length > 0
                && typeof window[$.trim(data)] === 'object') {
                ajaxObj = window[$.trim(data)];
            }
            else {
                data = $(o).attr('data-ajax-url');
                if (typeof data !== 'undefined' && data.length > 0) {
                    ajaxObj = {
                        url: $.trim(data)
                    };
                }
            }

            var cache = undefined;
            var abortcache = false;
            if (typeof ajaxObj !== 'undefined') {

                /*#region cache data*/

                data = $(o).attr('data-ajax-cacheExclusive');
                if (typeof data !== 'undefined' && data.length > 0) {
                    var isExc = $.trim(data).toLowerCase() === 'true' ? true : false;
                    if (isExc === true) {
                        cache = BootStrap_NET.ExtraDropdown.cacheObject.findCache($(o).attr('id'));
                        if (typeof cache !== 'undefined')
                            abortcache = true;
                    }
                }

                if (typeof cache === 'undefined') {
                    data = $(o).attr('data-ajax-cacheOnLoad');
                    var cacheSetting = {};
                    if (typeof data !== 'undefined' && data.length > 0) {
                        cacheSetting['cacheOnLoad'] = $.trim(data).toLowerCase() === 'true' ? true : false;
                        $(o).removeAttr('data-ajax-cacheOnLoad');
                    }
                    data = $(o).attr('data-ajax-cacheNumerPage');
                    if (typeof data !== 'undefined' && data.length > 0) {
                        cacheSetting['cacheNumerPage'] = parseInt($.trim(data)) || 0;
                        $(o).removeAttr('data-ajax-cacheNumerPage');
                    }

                    data = $(o).attr('data-ajax-cacheKeyWordMinLength');
                    if (typeof data !== 'undefined' && data.length > 0) {
                        cacheSetting['cacheKeyWordMinLength'] = parseInt($.trim(data)) || 0;
                        $(o).removeAttr('data-ajax-cacheKeyWordMinLength');
                    }

                    data = $(o).attr('data-ajax-autoCache');
                    if (typeof data !== 'undefined' && data.length > 0) {
                        cacheSetting['autoCache'] = $.trim(data).toLowerCase() === 'false' ? false : true;
                        $(o).removeAttr('data-ajax-autoCache');
                    }

                    if ($.isEmptyObject(cacheSetting) === false) {
                        cacheSetting['id'] = $(o).attr('id');
                        var cacheLoad = cacheSetting.cacheOnLoad || false;
                        var autoCache = true;
                        if (typeof cacheSetting.autoCache !== 'undefined')
                            autoCache = cacheSetting.autoCache;

                        cache = BootStrap_NET.ExtraDropdown.cacheObject.addCacheForElement(cacheSetting.id,
                               cacheSetting.cacheNumerPage || (cacheLoad === true ? 1 : -1), cacheLoad,
                               cacheSetting.cacheKeyWordMinLength || -1, autoCache);
                    }
                }

                /*#endregion*/

                /*#region ajax info*/

                data = $(o).attr('data-ajax-infourl');
                if (typeof data !== 'undefined' && data.length > 0) {
                    ajaxObj['infourl'] = $.trim(data);
                    $(o).removeAttr('data-ajax-infourl');
                }

                data = $(o).attr('data-ajax-infodataType');
                if (typeof data !== 'undefined' && data.length > 0) {
                    ajaxObj['infodataType'] = $.trim(data);
                    $(o).removeAttr('data-ajax-infodataType');
                }

                data = $(o).attr('data-ajax-infomethodType');
                if (typeof data !== 'undefined' && data.length > 0) {
                    ajaxObj['infomethodType'] = $.trim(data);
                    $(o).removeAttr('data-ajax-infomethodType');
                }


                data = $(o).attr('data-ajax-infocontentType');
                if (typeof data !== 'undefined' && data.length > 0) {
                    ajaxObj['infocontentType'] = $.trim(data);
                    $(o).removeAttr('data-ajax-infocontentType');
                } else
                    ajaxObj['infocontentType'] = 'application/x-www-form-urlencoded; charset=UTF-8';

                /*#endregion*/

                /*#region ajax data*/

                data = $(o).attr('data-ajax-dataType');
                if (typeof data !== 'undefined' && data.length > 0) {
                    ajaxObj['dataType'] = $.trim(data);
                    $(o).removeAttr('data-ajax-dataType');
                }
                else
                    ajaxObj['dataType'] = 'json';

                data = $(o).attr('data-ajax-datamethodType');
                if (typeof data !== 'undefined' && data.length > 0) {
                    ajaxObj['type'] = $.trim(data);
                    $(o).removeAttr('data-ajax-datamethodType');
                }

                data = $(o).attr('data-ajax-datacontentType');
                if (typeof data !== 'undefined' && data.length > 0) {
                    ajaxObj['contentType'] = $.trim(data);
                    $(o).removeAttr('data-ajax-datacontentType');
                } else
                    ajaxObj['contentType'] = 'application/x-www-form-urlencoded; charset=UTF-8';

                /*#endregion*/

                data = $(o).attr('data-ajax-pageSize');
                if (typeof data !== 'undefined' && data.length > 0) {
                    ajaxObj['pageSize'] = parseInt($.trim(data)) || 10;
                    $(o).removeAttr('data-ajax-pageSize');
                }
                else
                    ajaxObj['pageSize'] = 10;

                /*#region function*/

                data = $(o).attr('data-ajax-processResultsFunction');
                if (typeof data !== 'undefined' && data.length > 0
                && typeof window[$.trim(data)] === 'function') {
                    ajaxObj['processResults'] = window[$.trim(data)];
                    $(o).removeAttr('data-ajax-processResultsFunction');
                }
                else {
                    ajaxObj['processResults'] = function (data, params) {
                        //console.log(data, params);
                        // parse the results into the format expected by Select2
                        // since we are using custom formatting functions we do not need to
                        // alter the remote JSON data, except to indicate that infinite
                        // scrolling can be used
                        params.page = params.page || 1;

                        var dataConvert = undefined;
                        if (typeof data.d !== 'undefined' && data.d !== null)
                            dataConvert = data.d;
                        else
                            dataConvert = data;

                        if (typeof dataConvert.data === 'undefined' || dataConvert.data === null)
                            console.log('Not found "data" return.');
                        if (typeof dataConvert.total === 'undefined' || dataConvert.total === null)
                            console.log('Not found "total" return.');
                        var newobj = {
                            results: dataConvert.data || [],
                            pagination: {
                                more: (params.page * 10) < (dataConvert.total || 0)
                            }
                        };
                        return newobj;
                    };
                }

                data = $(o).attr('data-ajax-dataFunction');
                if (typeof data !== 'undefined' && data.length > 0
                && typeof window[$.trim(data)] === 'function') {
                    ajaxObj['data'] = window[$.trim(data)];
                    $(o).removeAttr('data-ajax-dataFunction');
                }
                else {
                    //set defaulf function
                    ajaxObj['data'] = function (params) {
                        var obj = {
                            keyword: params.term || '', // search term
                            page: params.page || 1,
                            page_limit: ajaxObj['pageSize']
                        };

                        if (ajaxObj['contentType'].toLowerCase().indexOf('application/json') >= 0)
                            return JSON.stringify(obj);
                        else
                            return obj;
                    }
                }

                if (hasCascading === false) {
                    data = $(o).attr('data-ajax-beforeSendFunction');
                    if (typeof data !== 'undefined' && data.length > 0
                    && typeof window[$.trim(data)] === 'function') {
                        ajaxObj['beforeSend'] = window[$.trim(data)];
                        $(o).removeAttr('data-ajax-beforeSendFunction');
                    }
                    else {
                        ajaxObj['beforeSend'] = function (xhr, ajaxOpts, dataParams, cacheParam) {

                            if (typeof cacheParam !== 'undefined') {
                                /*#region get paging and keyword*/

                                var page = 1;
                                var ke = '';

                                if (typeof ajaxOpts.data.page !== 'undefined')
                                    page = typeof (ajaxOpts.data.page) === 'number' ? ajaxOpts.data.page : (parseInt(ajaxOpts.data.page) || 1);
                                else {
                                    try {
                                        var ob = JSON.parse(ajaxOpts.data);
                                        page = typeof (ob.page) === 'number' ? ob.page : (parseInt(ob.page) || 1);
                                    }
                                    catch (ex) {
                                        //query string
                                        var p = BootStrap_NET.ExtraDropdown.commonFunction.getDataQuery('page',
                                            (ajaxOpts.data.indexOf('?') === 0 ? '' : '?') + ajaxOpts.data);
                                        if (p !== null && p.length > 0)
                                            page = parseInt(p) || 1;
                                    }
                                }

                                if (typeof dataParams !== 'undefined' && typeof dataParams.term !== 'undefined')
                                    ke = dataParams.term || '';

                                /*#endregion*/

                                var fullKey = ajaxOpts.url + '_' + ke + '_' + page;

                                if (cacheParam.exist(fullKey)) {
                                    var sel = $(o).data('select2');
                                    if (typeof sel !== 'undefined') {
                                        //sel.results.loading = false;
                                        //sel.results.hideLoading();

                                        var cacheData = cacheParam.get(fullKey);
                                        var newobj = undefined;
                                        if ($.isFunction(ajaxOpts.processResults) === true)
                                            newobj = ajaxOpts.processResults(cacheData[0], cacheData[1]);

                                        if (page === 1) {
                                            sel.trigger('results:all', { data: newobj, query: cacheData[1] });
                                        }
                                        else
                                            sel.trigger('results:append', { data: newobj, query: cacheData[1] });
                                    }
                                    xhr.abort();

                                    return false;
                                }
                            }
                            return true;
                        };
                    }
                }
                else {
                    $(o).removeAttr('data-ajax-beforeSendFunction');
                    ajaxObj['beforeSend'] = function (xhr, ajaxOpts, dataParams, cacheParam) {
                        console.log('BeforeSendSub : ', xhr, ajaxOpts, dataParams, cacheParam);

                        var keyw = $(opts['ajaxCascadingWith']).val() || '';

                        if (keyw.length === 0) {
                            if ($.isFunction(xhr.abort) === true)
                                xhr.abort();

                            var api = $(o).data('select2');
                            //api.$container.removeClass('select2-container--open select2-container--focus');
                            //api.dropdown._hideDropdown();

                            api.trigger('results:all', {
                                data: {
                                    results: [],
                                    pagination: {
                                        more: false
                                    }
                                },
                                query: {}
                            });

                        }
                        else {
                            if (typeof cacheParam !== 'undefined') {

                                var page = 1;

                                var fullKey = ajaxOpts.url + '_' + keyw + '_' + page;

                                if (cacheParam.exist(fullKey)) {
                                    var api = $(o).data('select2');
                                    if (typeof api !== 'undefined') {
                                        //sel.results.loading = false;
                                        //sel.results.hideLoading();

                                        var cacheData = cacheParam.get(fullKey);
                                        var newobj = undefined;
                                        if ($.isFunction(ajaxOpts.processResults) === true)
                                            newobj = ajaxOpts.processResults(cacheData[0], cacheData[1]);

                                        api.trigger('results:all', { data: newobj, query: cacheData[1] });
                                    }

                                    xhr.abort();
                                    return false;
                                }
                            }
                        }
                    };
                }

                data = $(o).attr('data-ajax-transportFunction');
                if (typeof data !== 'undefined' && data.length > 0
                && typeof window[$.trim(data)] === 'function') {
                    ajaxObj['transport'] = window[$.trim(data)];
                    $(o).removeAttr('data-ajax-transportFunction');
                }

                data = $(o).attr('data-ajax-errorFunction');
                if (typeof data !== 'undefined' && data.length > 0
                && typeof window[$.trim(data)] === 'function') {
                    ajaxObj['error'] = window[$.trim(data)];
                    $(o).removeAttr('data-ajax-errorFunction');
                }


                if (hasCascading === false) {
                    data = $(o).attr('data-ajax-successFunction');
                    if (typeof data !== 'undefined' && data.length > 0
                    && typeof window[$.trim(data)] === 'function') {
                        ajaxObj['success'] = window[$.trim(data)];
                        $(o).removeAttr('data-ajax-successFunction');
                    }
                }
                else {
                    $(o).removeAttr('data-ajax-successFunction');
                    ajaxObj['success'] = function (data, ajaxOpts, dataParams, cacheParam) {
                        console.log('call success');
                        if (typeof cacheParam !== 'undefined') {
                            var dataUrl = this.url;
                            var page = dataParams.page || 1;
                            var keyw = $(opts['ajaxCascadingWith']).val() || '';

                            cacheParam.set(dataUrl, keyw, page, [data, dataParams], function () { });
                        }
                    }
                }

                /*#endregion*/

                opts.ajax = ajaxObj;
            }
            data = $(o).attr('data-EncryptHtml');
            if (typeof data !== 'undefined' && data.length > 0) {
                if ($.trim(data).toLowerCase() === 'true') {
                    opts["escapeMarkup"] = function (markup) {
                        return markup;
                    }

                    opts["templateResult"] = function (data) {
                        return data.html;
                    }

                    opts["templateSelection"] = function (data) {
                        return data.text;
                    }
                }
            }
           
            /*#endregion*/

            data = $(o).attr('data-templateResultFunction');
            if (typeof data !== 'undefined' && data.length > 0
            && typeof window[$.trim(data)] === 'function') {
                opts['templateResult'] = window[$.trim(data)];
                $(o).removeAttr('data-templateResultFunction');
            }

            data = $(o).attr('data-templateSelectionFunction');
            if (typeof data !== 'undefined' && data.length > 0
            && typeof window[$.trim(data)] === 'function') {
                opts['templateSelection'] = window[$.trim(data)];
                $(o).removeAttr('data-templateSelectionFunction');
            }

            data = $(o).attr('onchange');
            if (typeof data !== 'undefined' && data.length > 0) {
                $(o).attr('data-onchange', data).removeAttr('onchange');
            }

            /*#endregion*/

            //get value from hidden field to set
            var laterSet = false;
            var dataHidden = BootStrap_NET.ExtraDropdown.mainFunction.getHiddenData(o);
            //console.log('dataHidden : ',dataHidden);
            if ($.isArray(opts['data']) === true) {
                var dataHidden = BootStrap_NET.ExtraDropdown.mainFunction.getHiddenData(o);
                var dataHDF = dataHidden.id;
                if (typeof dataHDF !== 'undefined') {
                    if ($.isArray(dataHDF) === true) {
                        $.each(opts['data'], function (ii, oo) {
                            $.each(dataHDF, function (ind, ob) {
                                if (oo.id === ob) {
                                    oo.selected = true;
                                    return false;
                                }
                            });
                        });
                    }
                    else {
                        $.each(opts['data'], function (ii, oo) {
                            if (oo.id === dataHDF) {
                                oo.selected = true;
                                return false;
                            }
                        });
                    }
                }
            }
            else {
                var dataHDF = dataHidden.id;
                var dataText = dataHidden.text;

                if ((typeof dataHDF !== 'undefined' && typeof dataText === 'undefined')
                    || (typeof dataHDF === 'undefined' && typeof dataText !== 'undefined')) {
                    //get info
                    laterSet = true;
                }
                else
                    BootStrap_NET.ExtraDropdown.mainFunction.setValueBack(o);
            }

            $(o).select2(opts);

            if (laterSet === true) {
                var dataHDF = dataHidden.id;
                var dataText = dataHidden.text;
                var type = 'id';
                var val = dataHDF;
                if (typeof dataHDF === 'undefined') {
                    type = 'txt';
                    val = dataText;
                }
                BootStrap_NET.ExtraDropdown.mainFunction.GetInfo(val, type, o);
            }

            var oldchangeev = BootStrap_NET.ExtraDropdown.commonFunction.findEventHandlers('change', o);

            if (typeof oldchangeev !== 'undefined' && oldchangeev.length > 0) {
                var orev = oldchangeev[0].events[0];
                var oldchange = orev.handler;
                //var handlename = orev.namespace ? (orev.origType + "." + orev.namespace) : orev.origType;
                //console.log(handlename, oldchange, orev);
                if ($.isFunction(oldchange) === true) {
                    orev.handler = function (x, y) {
                        oldchange.apply(this, arguments);

                        var api = $(o).data('select2');
                        if (typeof api !== 'undefined') {
                            var dataToSet = api.data();

                            var idforhdf = $(this).attr('data-hdfValue');
                            var textforhdf = $(this).attr('data-hdfText');
                            var hdfid = $(this).parent().find('input[type="hidden"][id$="' + idforhdf + '"]');
                            var hdftext = $(this).parent().find('input[type="hidden"][id$="' + textforhdf + '"]');

                            if (typeof dataToSet !== 'undefined') {
                                //console.log(dataToSet, this);

                                var ismulti = $(this).prop('multiple');

                                if (hdfid.length > 0 && hdftext.length > 0
                                    && $.isArray(dataToSet) === true) {
                                    {
                                        if (ismulti === true) {
                                            var arid = [];
                                            var artext = [];
                                            $.each(dataToSet, function (ind, ob) {
                                                if ($.inArray(ob.id, arid) < 0)
                                                    arid.push(ob.id);

                                                if ($.inArray(ob.text, artext) < 0)
                                                    artext.push(ob.text);
                                            });

                                            //console.log(arid, artext);
                                            if (arid.length > 0)
                                                hdfid.val("['" + arid.join("','") + "']");
                                            else
                                                hdfid.val("[]");
                                            if (artext.length > 0)
                                                hdftext.val("['" + artext.join("','") + "']");
                                            else
                                                hdftext.val("[]");
                                        }
                                        else {
                                            if (dataToSet.length > 0) {
                                                hdfid.val(dataToSet[0].id);
                                                hdftext.val(dataToSet[0].text);
                                            }
                                            else {
                                                hdfid.val('');
                                                hdftext.val('');
                                            }
                                        }
                                    }
                                }
                                else {

                                }
                            }
                            else {
                                if (hdfid.length > 0 && hdftext.length > 0) {
                                    var ismulti = $(this).prop('multiple');
                                    if (ismulti === false) {
                                        hdfid.val('');
                                        hdftext.val('');
                                    }
                                    else {
                                        hdfid.val('[]');
                                        hdftext.val('[]');
                                    }
                                }
                            }
                        }


                        var onchangeattr = $(o).attr('data-onchange');
                        if (onchangeattr && onchangeattr.length > 0)
                            eval(onchangeattr);
                    }
                }
            }

            //change color on show
            $(o).on("select2:open", function () {

                var prefix = 'has-';
                var par = $(document.body);
                if (typeof opts['dropdownParent'] !== 'undefined')
                    par = opts['dropdownParent'];
                var ctn = par.find(" > .select2-container");
                if ($(this).parents("[class*='" + prefix + "']").length) {
                    var classNames = $(this).parents("[class*='" + prefix + "']")[0].className.split(/\s+/);
                    for (var i = 0; i < classNames.length; ++i) {
                        if (classNames[i].match(prefix)) {
                            ctn.addClass(classNames[i]);
                        }
                    }
                }
                else
                    BootStrap_NET.ExtraDropdown.mainFunction.RemoveClassColor(ctn, prefix);

                prefix = 'select2-';
                if ($(this).parents("[class*='" + prefix + "']").length) {
                    var classNames = $(this).parents("[class*='" + prefix + "']")[0].className.split(/\s+/);

                    for (var i = 0; i < classNames.length; ++i) {
                        if (classNames[i].match(prefix)) {
                            ctn.addClass(classNames[i]);
                        }
                    }
                }
                else
                    BootStrap_NET.ExtraDropdown.mainFunction.RemoveClassColor(ctn, prefix);
            });

            data = $(o).attr('required');
            if (typeof data !== 'undefined') {
                $(o).on('select2:close', function (e) {
                    BootStrap_NET.ExtraDropdown.mainFunction.CheckValid(o);
                });
            }

            var initDoneFunc = opts['afterInitFunction'];
            if ($.isFunction(initDoneFunc) === true)
                initDoneFunc(o);

            if (typeof cache !== 'undefined' && cache._cacheOnLoad === true) {
                if (abortcache === false) {
                    setTimeout(function () {
                        $(o).data('select2').trigger('query', {});
                        BootStrap_NET.ExtraDropdown.mainFunction.HideLoading(o);
                    }, 150);
                }
            }
        },
        getHiddenData: function (el) {
            var returnData = { id: undefined, text: undefined };

            if (typeof el !== 'undefined') {
                var dataHDF = undefined;
                var idforhdf = $(el).attr('data-hdfValue');
                if (typeof idforhdf !== 'undefined' && idforhdf.length > 0) {
                    var hdf = $(el).parent().find('input[type="hidden"][id="' + idforhdf + '"]');

                    if (hdf.length > 0 && hdf.val().length > 0) {
                        var objArr = undefined;
                        if (hdf.val().indexOf('[') === 0) {
                            try {
                                objArr = eval(hdf.val());
                            }
                            catch (ex) {

                            }
                        }

                        if ($.isArray(objArr) === true)
                            dataHDF = objArr;
                        else
                            dataHDF = hdf.val();
                    }
                }

                returnData.id = dataHDF;

                //console.log('dataHDF : ', dataHDF);
                if (typeof dataHDF !== 'undefined') {
                    var dataText = undefined;
                    var textforhdf = $(el).attr('data-hdfText');
                    if (typeof textforhdf !== 'undefined' && textforhdf.length > 0) {
                        var hdf1 = $(el).parent().find('input[type="hidden"][id="' + textforhdf + '"]');

                        if (hdf1.length > 0 && hdf1.val().length > 0) {
                            var objArr1 = undefined;
                            if (hdf1.val().startsWith('[') && hdf1.val().endsWith(']')) {
                                try {
                                    objArr1 = eval(hdf1.val());
                                }
                                catch (ex) {
                                    objArr1 = undefined;
                                    //console.log('error ', ex);
                                }
                            }
                            //console.log('hdf text val ', hdf1.val());
                            if ($.isArray(objArr1) === true)
                                dataText = objArr1;
                            else
                                dataText = hdf1.val();
                        }
                    }

                    returnData.text = dataText;
                }
            }

            return returnData;
        },
        setValueBack: function (el) {
            if (typeof el !== 'undefined') {
                var data = BootStrap_NET.ExtraDropdown.mainFunction.getHiddenData(el);
                var dataHDF = data.id;
                var dataText = data.text;
                if ($.isArray(dataHDF) === true) {
                    if (dataHDF.length > 0) {
                        var op = [];
                        if ($(el).has('option').length > 0) {
                            $.each(dataHDF, function (i, o) {
                                op = $(el).find('option[value="' + o + '"]');
                                if (op.length > 0)
                                    op.attr('selected', 'selected');
                            });
                        }
                        else {
                            for (var i = 0; i < dataHDF.length; i++) {
                                $(el).append('<option value="' + dataHDF[i] + '" selected="selected">' +
                                          (dataText.length > i ? dataText[i] : 'undefined') + '</option>');
                            }
                        }
                    }
                }
                else {
                    if (typeof dataHDF !== 'undefined' && dataHDF.length > 0) {
                        if ($(el).has('option').length > 0) {
                            var op = $(el).find('option[value="' + dataHDF + '"]');
                            if (op.length > 0)
                                op.attr('selected', 'selected');
                        }
                        else {
                            $(el).append('<option value="' + dataHDF + '" selected="selected">' +
                                      (dataText || 'undefined') + '</option>');
                        }
                    }
                }
            }
        },
        GetInfo: function (values, type, obj, isAppend) {
            if (typeof obj !== 'undefined') {
                var api = undefined;
                var el = undefined;

                if (obj instanceof jQuery || (typeof obj.tagName !== 'undefined' && obj.tagName === 'SELECT')) {
                    api = $(obj).data('select2');
                    el = $(obj);
                }
                else if (typeof obj.options !== 'undefined' && typeof obj.options.options !== 'undefined') {
                    api = obj;
                    el = $(api.$element);
                }

                if (typeof api.options.options.ajax !== 'undefined'
                    && typeof api.options.options.ajax.url !== 'undefined'
                    && api.options.options.ajax.url.length > 0) {
                    var ajaxSetup = $.extend({}, api.options.options.ajax);
                    var objArr = undefined;
                    try {
                        objArr = eval(values);
                    }
                    catch (ex) {

                    }
                    var str = '';
                    if ($.isArray(objArr) === true && objArr.length > 0)
                        str = objArr.join(',');
                    else
                        str = values;

                    if (typeof ajaxSetup['infomethodType'] !== 'undefined'
                        && ajaxSetup['infomethodType'].length > 0)
                        ajaxSetup['type'] = ajaxSetup['infomethodType'];
                    else if (typeof ajaxSetup['type'] === 'undefined' || ajaxSetup['type'].length === 0)
                        ajaxSetup['type'] = 'post';

                    if (typeof ajaxSetup['infodataType'] !== 'undefined'
                        && ajaxSetup['infodataType'].length > 0)
                        ajaxSetup['dataType'] = ajaxSetup['infodataType'];
                    else if (typeof ajaxSetup['dataType'] === 'undefined' || ajaxSetup['dataType'].length === 0)
                        ajaxSetup['dataType'] = 'json';

                    if (typeof ajaxSetup['infocontentType'] !== 'undefined'
                        && ajaxSetup['infocontentType'].length > 0)
                        ajaxSetup['contentType'] = ajaxSetup['infocontentType'];

                    if (ajaxSetup['contentType'].indexOf('application/json') >= 0)
                        ajaxSetup['data'] = (typeof type === 'undefined' || type.toLowerCase() === 'id') ?
                            JSON.stringify({ ids: str }) : JSON.stringify({ txts: str });
                    else
                        ajaxSetup['data'] = (typeof type === 'undefined' || type.toLowerCase() === 'id' ? 'ids=' : 'txts=') + str;

                    if (typeof ajaxSetup['infourl'] !== 'undefined'
                        && ajaxSetup['infourl'].length > 0)
                        ajaxSetup['url'] = ajaxSetup['infourl'];


                    ajaxSetup['success'] = function (data) {
                        if (typeof data !== 'undefined') {
                            var dataToSet1 = [];
                            var jddl = $(el);

                            if (typeof isAppend === 'boolean' && isAppend === true) {
                                dataToSet1 = jddl.val();
                            }

                            var textAttr = 'text';
                            var idAttr = 'id';
                            var dataConvert = undefined;
                            if (typeof data.d !== 'undefined')
                                dataConvert = data.d;
                            else
                                dataConvert = data;

                            if ($.isArray(dataConvert) === true) {
                                var dataO = undefined;
                                $.each(dataConvert, function (i, o) {
                                    dataO = BootStrap_NET.ExtraDropdown.mainFunction.getPropertyNotCaseInsensitive(o, idAttr);
                                    if (dataO === null)
                                        dataO = BootStrap_NET.ExtraDropdown.mainFunction.getPropertyNotCaseInsensitive(o, 'id');

                                    if (typeof dataO !== 'undefined' && dataO !== null) {
                                        if (jddl.find('option[value="' + dataO + '"]').length === 0) {
                                            jddl.append('<option value="' + dataO + '">' + (o[textAttr] || dataO) + '</option>');
                                        }

                                        if ($.inArray(dataO.toString(), dataToSet1) < 0)
                                            dataToSet1.push(dataO.toString());
                                    }
                                    else
                                        throw new Error('error, id attribute not found');
                                });
                            }
                            else {
                                var dataVal = BootStrap_NET.ExtraDropdown.mainFunction.getPropertyNotCaseInsensitive(dataConvert, idAttr);
                                if (dataVal === null)
                                    dataVal = BootStrap_NET.ExtraDropdown.mainFunction.getPropertyNotCaseInsensitive(dataConvert, 'id');
                                if (typeof dataVal !== 'undefined' && dataVal !== null) {
                                    if (jddl.find('option[value="' + dataVal + '"]').length === 0) {
                                        jddl.append('<option value="' + dataVal + '">' +
                                            (dataConvert[textAttr] || dataVal) + '</option>');
                                    }
                                }
                                dataToSet1.push(typeof dataVal !== 'undefined' && dataVal !== null ? dataVal.toString() : '');
                            }

                            //console.log('dataToSet1 : ', dataToSet1);
                            $(el).val(dataToSet1).trigger('change');
                            BootStrap_NET.ExtraDropdown.mainFunction.HideLoading(el);
                        }
                    }

                    $.ajax(ajaxSetup);
                }
            }
        },
        CheckValid: function (el) {
            if (typeof $.fn.validate === 'function') {
                return $(el).valid();
            }
            else {
                var val = $(el).val();
                if (val === null || val.length === 0) {
                    if ($(el).parent().hasClass('input-group') === false)
                        $(el).parent().addClass('has-error');
                    else
                        $(el).parent().parent().addClass('has-error');
                    return false;
                }
                else {
                    if ($(el).parent().hasClass('input-group') === false)
                        $(el).parent().removeClass('has-error');
                    else
                        $(el).parent().parent().removeClass('has-error');
                }
                return true;
            }
        },
        RemoveClassColor: function (el, prefix) {
            var color = BootStrap_NET.ExtraDropdown.data.color;
            if (typeof el !== 'undefined' && el.length > 0 &&
                $.isArray(color) === true && color.length > 0 &&
                typeof prefix !== 'undefined' && prefix.length > 0) {
                var classes = el.attr("class").split(' ');
                var newClass = [];
                $.each(classes, function (i, c) {
                    if (c.indexOf(prefix) >= 0) {
                        if ($.inArray(c, color) === -1)
                            newClass.push(c);
                    }
                    else
                        newClass.push(c);
                });
                if (newClass.length > 0)
                    $(el).attr('class', newClass.join(' '));
                else
                    $(el).removeClass();
            }
        },
        ShowLoading: function (ddl, callback) {
            if (typeof ddl !== 'undefined') {
                var sel = $(ddl).data('select2');
                sel.$container.addClass('select2-container--open select2-container--focus');
                sel.results.clear();
                sel.dropdown._showDropdown();
                sel.results.showLoading({});
                if (typeof callback === 'function')
                    callback();
            }
        },
        HideLoading: function (ddl) {
            if (typeof ddl !== 'undefined') {
                var sel = $(ddl).data('select2');
                sel.$container.removeClass('select2-container--open select2-container--focus');
                sel.dropdown._hideDropdown();
            }
        },
        LostIdException: function (message) {
            this.message = message;
            this.name = "LostIdException";
        },
        getPropertyNotCaseInsensitive: function (obj, property) {
            property = (property + "").toLowerCase();
            if (property.length > 0) {
                for (var p in obj) {
                    if (obj.hasOwnProperty(p) && property == (p + "").toLowerCase()) {
                        return obj[p];
                    }
                }
            }
            return null;
        }
    }
};

(function ($) {
    BootStrap_NET.ExtraDropdown.mainFunction.init();
    if (typeof Sys !== 'undefined' && typeof Sys.WebForms !== 'undefined')
        Sys.WebForms.PageRequestManager.getInstance().add_endRequest(BootStrap_NET.ExtraDropdown.mainFunction.init);
})(jQuery);

$(window).on("load", function () {
    setTimeout(function () {
        BootStrap_NET.ExtraDropdown.mainFunction.initWindowLoad();
    }, 100);
});
