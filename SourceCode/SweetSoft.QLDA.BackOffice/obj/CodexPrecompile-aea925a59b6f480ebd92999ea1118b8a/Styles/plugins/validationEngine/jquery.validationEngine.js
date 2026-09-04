! function (e) {
	"use strict";
	var t = {
		init: function (a) {
			return this.data("jqv") && null != this.data("jqv") || (a = t._saveOptions(this, a), e(document).on("click", ".formError", function () {
				e(this).fadeOut(150, function () {
					e(this).closest(".formError").remove()
				})
			}), e(document).on("click", ".el-error", function () {
				e(this).fadeOut(150, function () {
					e(this).closest(".el-error").remove()
				})
			})), this
		},
		attach: function (a) {
			var r;
			return (r = a ? t._saveOptions(this, a) : this.data("jqv")).validateAttribute = this.find("[data-validation-engine*=validate]").length ? "data-validation-engine" : "class", r.binded && (this.on(r.validationEventTrigger, "[" + r.validateAttribute + "*=validate]:not([type=checkbox]):not([type=radio]):not(.datepicker)", t._onFieldEvent), this.on("keypress", "[" + r.validateAttribute + "*=validate][valid-keypress=true]:not([type=checkbox]):not([type=radio]):not(.datepicker)", t._onFieldEvent), this.on("click", "[" + r.validateAttribute + "*=validate][type=checkbox],[" + r.validateAttribute + "*=validate][type=radio]", t._onFieldEvent), this.on(r.validationEventTrigger, "[" + r.validateAttribute + "*=validate][class*=datepicker]",
				{
					delay: 300
				}, t._onFieldEvent)), r.autoPositionUpdate && e(window).bind("resize",
					{
						noAnimation: !0,
						formElem: this
					}, t.updatePromptsPosition), this.on("click", "a[data-validation-engine-skip], a[class*='validate-skip'], button[data-validation-engine-skip], button[class*='validate-skip'], input[data-validation-engine-skip], input[class*='validate-skip']", t._submitButtonClick), this.removeData("jqv_submitButton"), this.on("submit", t._onSubmitEvent), this
		},
		detach: function () {
			var a = this.data("jqv");
			return this.off(a.validationEventTrigger, "[" + a.validateAttribute + "*=validate]:not([type=checkbox]):not([type=radio]):not(.datepicker)", t._onFieldEvent), this.off("click", "[" + a.validateAttribute + "*=validate][type=checkbox],[" + a.validateAttribute + "*=validate][type=radio]", t._onFieldEvent), this.off(a.validationEventTrigger, "[" + a.validateAttribute + "*=validate][class*=datepicker]", t._onFieldEvent), this.off("submit", t._onSubmitEvent), this.removeData("jqv"), this.off("click", "a[data-validation-engine-skip], a[class*='validate-skip'], button[data-validation-engine-skip], button[class*='validate-skip'], input[data-validation-engine-skip], input[class*='validate-skip']", t._submitButtonClick), this.removeData("jqv_submitButton"), a.autoPositionUpdate && e(window).off("resize", t.updatePromptsPosition), this
		},
		validate: function (a) {
			var r, i = e(this),
				s = null;
			if (i.is("form") || i.hasClass("validationEngineContainer")) {
				if (i.hasClass("validating")) return !1;
				i.addClass("validating"), r = a ? t._saveOptions(i, a) : i.data("jqv");
				s = t._validateFields(this);
				setTimeout(function () {
					i.removeClass("validating")
				}, 100), s && r.onSuccess ? r.onSuccess() : !s && r.onFailure && r.onFailure()
			}
			else {
				if (!i.is("form") && !i.hasClass("validationEngineContainer")) {
					var o = i.closest("form, .validationEngineContainer");
					return r = o.data("jqv") ? o.data("jqv") : e.validationEngine.defaults, (s = t._validateField(i, r)) && r.onFieldSuccess ? r.onFieldSuccess() : r.onFieldFailure && r.InvalidFields.length > 0 && r.onFieldFailure(), !s
				}
				i.removeClass("validating")
			}
			return r.onValidationComplete ? !!r.onValidationComplete(o, s) : s
		},
		updatePromptsPosition: function (a) {
			if (a && this == window) var r = a.data.formElem,
				i = a.data.noAnimation;
			else r = e(this.closest("form, .validationEngineContainer"));
			var s = r.data("jqv");
			return s || (s = t._saveOptions(r, s)), r.find("[" + s.validateAttribute + "*=validate]").not(":disabled").each(function () {
				var a = e(this);
				s.prettySelect && a.is(":hidden") && (a = r.find("#" + s.usePrefix + a.attr("id") + s.useSuffix));
				var o = t._getPrompt(a),
					n = e(o).find(".formErrorContent").html();
				o && t._updatePrompt(a, e(o), n, void 0, !1, s, i)
			}), this
		},
		showPrompt: function (e, a, r, i) {
			var s = this.closest("form, .validationEngineContainer").data("jqv");
			return s || (s = t._saveOptions(this, s)), r && (s.promptPosition = r), s.showArrow = 1 == i, e = "<li>" + e + "</li>", t._showPrompt(this, e, a, !1, s), this
		},
		hide: function () {
			var a = e(this).closest("form, .validationEngineContainer"),
				r = a.data("jqv");
			r || (r = t._saveOptions(a, r));
			var i, s = r && r.fadeDuration ? r.fadeDuration : .3;
			return i = a.is("form") || a.hasClass("validationEngineContainer") ? "parentForm" + t._getClassName(e(a).attr("id")) : t._getClassName(e(a).attr("id")) + "formError", e("." + i).fadeTo(s, 0, function () {
				e(this).closest(".formError").remove(), e(this).closest(".el-error").remove()
			}), this
		},
		hideAll: function () {
			var t = this,
				a = t.data("jqv"),
				r = a ? a.fadeDuration : 300;
			return e(".formError").fadeTo(r, 0, function () {
				e(this).closest(".formError").remove(), t.find(".ext-error").removeClass("ext-error"), t.removeClass("ext-error")
			}), e(".el-error").fadeTo(r, 0, function () {
				e(this).closest(".el-error").remove(), t.find(".ext-error").removeClass("ext-error"), t.removeClass("ext-error")
			}), this
		},
		_onFieldEvent: function (a) {
			var r = e(this),
				i = r.closest("form, .validationEngineContainer"),
				s = i.data("jqv");
			s || (s = t._saveOptions(i, s)), s.eventTrigger = "field", 1 == s.notEmpty ? r.val().length > 0 && window.setTimeout(function () {
				t._validateField(r, s)
			}, a.data ? a.data.delay : 0) : window.setTimeout(function () {
				t._validateField(r, s)
			}, a.data ? a.data.delay : 0)
		},
		_onSubmitEvent: function () {
			var a = e(this),
				r = a.data("jqv");
			if (a.data("jqv_submitButton")) {
				var i = e("#" + a.data("jqv_submitButton"));
				if (i && i.length > 0 && (i.hasClass("validate-skip") || "true" == i.attr("data-validation-engine-skip"))) return !0
			}
			r.eventTrigger = "submit";
			var s = t._validateFields(a);
			return s && r.ajaxFormValidation ? (t._validateFormWithAjax(a, r), !1) : r.onValidationComplete ? !!r.onValidationComplete(a, s) : s
		},
		_checkAjaxStatus: function (t) {
			var a = !0;
			return e.each(t.ajaxValidCache, function (e, t) {
				if (!t) return a = !1, !1
			}), a
		},
		_checkAjaxFieldStatus: function (e, t) {
			return 1 == t.ajaxValidCache[e]
		},
		_validateFields: function (a) {
			var r = a.data("jqv"),
				i = !1;
			a.trigger("jqv.form.validating");
			var s = null;
			if (a.find("[" + r.validateAttribute + "*=validate]").not(":disabled").each(function () {
				var o = e(this),
					n = [];
				if (e.inArray(o.attr("name"), n) < 0) {
					if ((i |= t._validateField(o, r)) && null == s && (o.is(":hidden") && r.prettySelect ? s = o = a.find("#" + r.usePrefix + t._jqSelector(o.attr("id")) + r.useSuffix) : (o.data("jqv-prompt-at") instanceof jQuery ? o = o.data("jqv-prompt-at") : o.data("jqv-prompt-at") && (o = e(o.data("jqv-prompt-at"))), s = o)), r.doNotShowAllErrosOnSubmit) return !1;
					if (n.push(o.attr("name")), 1 == r.showOneMessage && i) return !1
				}
			}), a.trigger("jqv.form.result", [i]), i) {
				if (r.scroll) {
					var o = s.offset().top,
						n = s.offset().left,
						l = r.promptPosition;
					if ("string" == typeof l && -1 != l.indexOf(":") && (l = l.substring(0, l.indexOf(":"))), "bottomRight" != l && "bottomLeft" != l) {
						var d = t._getPrompt(s);
						d && (o = d.offset().top)
					}
					if (r.scrollOffset && (o -= r.scrollOffset), r.isOverflown) {
						var u = e(r.overflownDIV);
						if (!u.length) return !1;
						o += u.scrollTop() + -parseInt(u.offset().top) - 5, e(r.overflownDIV).filter(":not(:animated)").animate(
							{
								scrollTop: o
							}, 1100, function () {
							r.focusFirstField && s.focus()
						})
					}
					else e("html, body").animate(
						{
							scrollTop: o
						}, 1100, function () {
						r.focusFirstField && s.focus()
					}), e("html, body").animate(
						{
							scrollLeft: n
						}, 1100)
				}
				else r.focusFirstField && s.focus();
				return !1
			}
			return !0
		},
		_validateFormWithAjax: function (a, r) {
			var i = a.serialize(),
				s = r.ajaxFormValidationMethod ? r.ajaxFormValidationMethod : "GET",
				o = r.ajaxFormValidationURL ? r.ajaxFormValidationURL : a.attr("action"),
				n = r.dataType ? r.dataType : "json";
			e.ajax(
				{
					type: s,
					url: o,
					cache: !1,
					dataType: n,
					data: i,
					form: a,
					methods: t,
					options: r,
					beforeSend: function () {
						return r.onBeforeAjaxFormValidation(a, r)
					},
					error: function (e, a) {
						r.onFailure ? r.onFailure(e, a) : t._ajaxError(e, a)
					},
					success: function (i) {
						if ("json" == n && !0 !== i) {
							for (var s = !1, o = 0; o < i.length; o++) {
								var l = i[o],
									d = l[0],
									u = e(e("#" + d)[0]);
								if (1 == u.length) {
									var c = l[2];
									if (1 == l[1])
										if ("" != c && c) {
											if (r.allrules[c]) (f = r.allrules[c].alertTextOk) && (c = f);
											r.showPrompts && t._showPrompt(u, c, "pass", !1, r, !0)
										}
										else t._closePrompt(u);
									else {
										var f;
										if (s |= !0, r.allrules[c]) (f = r.allrules[c].alertText) && (c = f);
										r.showPrompts && t._showPrompt(u, c, "", !1, r, !0)
									}
								}
							}
							r.onAjaxFormComplete(!s, a, i, r)
						}
						else r.onAjaxFormComplete(!0, a, i, r)
					}
				})
		},
		_validateField: function (a, r, i) {
			if (a.attr("id") || (a.attr("id", "form-validation-field-" + e.validationEngine.fieldIdCounter), ++e.validationEngine.fieldIdCounter), a.hasClass(r.ignoreFieldsWithClass)) return !1;
			if (!r.validateNonVisibleFields && (a.is(":hidden") && !r.prettySelect || a.parent().is(":hidden"))) return !1;
			var s = a.attr(r.validateAttribute),
				o = /validate\[(.*)\]/.exec(s);
			if (!o) return !1;
			var n = o[1],
				l = n.split(/\[|,|\]/),
				d = a.attr("name"),
				u = "",
				c = "",
				f = !1,
				v = !1;
			r.isError = !1, r.showArrow = !0, r.maxErrorsPerField > 0 && (v = !0);
			for (var p = e(a.closest("form, .validationEngineContainer")), m = 0; m < l.length; m++) l[m] = l[m].toString().replace(" ", ""), "" === l[m] && delete l[m];
			m = 0;
			for (var g = 0; m < l.length; m++) {
				if (v && g >= r.maxErrorsPerField) {
					if (!f) {
						var h = e.inArray("required", l);
						f = -1 != h && h >= m
					}
					break
				}
				var x = void 0;
				switch (l[m]) {
					case "required":
						f = !0, x = t._getErrorMessage(p, a, l[m], l, m, r, t._required);
						break;
					case "custom":
						x = t._getErrorMessage(p, a, l[m], l, m, r, t._custom);
						break;
					case "groupRequired":
						var _ = "[" + r.validateAttribute + "*=" + l[m + 1] + "]",
							C = p.find(_).eq(0);
						C[0] != a[0] && (t._validateField(C, r, i), r.showArrow = !0), (x = t._getErrorMessage(p, a, l[m], l, m, r, t._groupRequired)) && (f = !0), r.showArrow = !1;
						break;
					case "ajax":
						(x = t._ajax(a, l, m, r)) && (c = "load");
						break;
					case "minSize":
						x = t._getErrorMessage(p, a, l[m], l, m, r, t._minSize);
						break;
					case "maxSize":
						x = t._getErrorMessage(p, a, l[m], l, m, r, t._maxSize);
						break;
					case "min":
						x = t._getErrorMessage(p, a, l[m], l, m, r, t._min);
						break;
					case "max":
						x = t._getErrorMessage(p, a, l[m], l, m, r, t._max);
						break;
					case "past":
						x = t._getErrorMessage(p, a, l[m], l, m, r, t._past);
						break;
					case "future":
						x = t._getErrorMessage(p, a, l[m], l, m, r, t._future);
						break;
					case "dateRange":
						_ = "[" + r.validateAttribute + "*=" + l[m + 1] + "]";
						r.firstOfGroup = p.find(_).eq(0), r.secondOfGroup = p.find(_).eq(1), (r.firstOfGroup[0].value || r.secondOfGroup[0].value) && (x = t._getErrorMessage(p, a, l[m], l, m, r, t._dateRange)), x && (f = !0), r.showArrow = !1;
						break;
					case "dateTimeRange":
						_ = "[" + r.validateAttribute + "*=" + l[m + 1] + "]";
						r.firstOfGroup = p.find(_).eq(0), r.secondOfGroup = p.find(_).eq(1), (r.firstOfGroup[0].value || r.secondOfGroup[0].value) && (x = t._getErrorMessage(p, a, l[m], l, m, r, t._dateTimeRange)), x && (f = !0), r.showArrow = !1;
						break;
					case "maxCheckbox":
						a = e(p.find("input[name='" + d + "']")), x = t._getErrorMessage(p, a, l[m], l, m, r, t._maxCheckbox);
						break;
					case "minCheckbox":
						a = e(p.find("input[name='" + d + "']")), x = t._getErrorMessage(p, a, l[m], l, m, r, t._minCheckbox);
						break;
					case "equals":
						x = t._getErrorMessage(p, a, l[m], l, m, r, t._equals);
						break;
					case "funcCall":
						x = t._getErrorMessage(p, a, l[m], l, m, r, t._funcCall);
						break;
					case "creditCard":
						x = t._getErrorMessage(p, a, l[m], l, m, r, t._creditCard);
						break;
					case "condRequired":
						void 0 !== (x = t._getErrorMessage(p, a, l[m], l, m, r, t._condRequired)) && (f = !0);
						break;
					case "funcCallRequired":
						void 0 !== (x = t._getErrorMessage(p, a, l[m], l, m, r, t._funcCallRequired)) && (f = !0)
				}
				var b = !1;
				if ("object" == typeof x) switch (x.status) {
						case "_break":
							b = !0;
							break;
						case "_error":
							x = x.message;
							break;
						case "_error_no_prompt":
							return !0
					}
				if (0 == m && 0 == n.indexOf("funcCallRequired") && void 0 !== x && (x.trim().length > 0 && (u += "<li>" + x + "</li>"), r.isError = !0, g++, b = !0), b) break;
				"string" == typeof x && (x.trim().length > 0 && (u += "<li>" + x + "</li>"), r.isError = !0, g++)
			} !f && !a.val() && a.val().length < 1 && e.inArray("equals", l) < 0 && (r.isError = !1);
			var T = a.prop("type"),
				E = a.data("promptPosition") || r.promptPosition;
			("radio" == T || "checkbox" == T) && p.find("input[name='" + d + "']").size() > 1 && (a = e("inline" === E ? p.find("input[name='" + d + "'][type!=hidden]:last") : p.find("input[name='" + d + "'][type!=hidden]:first")), r.showArrow = r.showArrowOnRadioAndCheckbox), a.is(":hidden") && r.prettySelect && (a = p.find("#" + r.usePrefix + t._jqSelector(a.attr("id")) + r.useSuffix, !1)), r.isError && r.showPrompts ? t._showPrompt(a, u, c, !1, r) : t._closePrompt(a), a.trigger("jqv.field.result", [a, r.isError, u]);
			var k = e.inArray(a[0], r.InvalidFields);
			return -1 == k ? r.isError && r.InvalidFields.push(a[0]) : r.isError || r.InvalidFields.splice(k, 1), t._handleStatusCssClasses(a, r), r.isError && r.onFieldFailure && r.onFieldFailure(a), !r.isError && r.onFieldSuccess && r.onFieldSuccess(a), r.isError
		},
		_handleStatusCssClasses: function (e, t) {
			t.addSuccessCssClassToField && e.removeClass(t.addSuccessCssClassToField), t.addFailureCssClassToField && e.removeClass(t.addFailureCssClassToField), t.addSuccessCssClassToField && !t.isError && e.addClass(t.addSuccessCssClassToField), t.addFailureCssClassToField && t.isError && e.addClass(t.addFailureCssClassToField)
		},
		_getErrorMessage: function (a, r, i, s, o, n, l) {
			var d = jQuery.inArray(i, s);
			"custom" !== i && "funcCall" !== i && "funcCallRequired" !== i || (i = i + "[" + s[d + 1] + "]", delete s[d]);
			var u, c = i,
				f = (r.attr("data-validation-engine") ? r.attr("data-validation-engine") : r.attr("class")).split(" ");
			if (null != (u = "future" == i || "past" == i || "maxCheckbox" == i || "minCheckbox" == i ? l(a, r, s, o, n) : l(r, s, o, n))) {
				var v = t._getCustomErrorMessage(e(r), f, c, n);
				v && (u = v)
			}
			return u
		},
		_getCustomErrorMessage: function (e, a, r, i) {
			var s = !1,
				o = /^custom\[.*\]$/.test(r) ? t._validityProp.custom : t._validityProp[r];
			if (null != o && null != (s = e.attr("data-errormessage-" + o))) return s;
			if (null != (s = e.attr("data-errormessage"))) return s;
			var n = "#" + e.attr("id");
			if (void 0 !== i.custom_error_messages[n] && void 0 !== i.custom_error_messages[n][r]) s = i.custom_error_messages[n][r].message;
			else if (a.length > 0)
				for (var l = 0; l < a.length && a.length > 0; l++) {
					var d = "." + a[l];
					if (void 0 !== i.custom_error_messages[d] && void 0 !== i.custom_error_messages[d][r]) {
						s = i.custom_error_messages[d][r].message;
						break
					}
				}
			s || void 0 === i.custom_error_messages[r] || void 0 === i.custom_error_messages[r].message || (s = i.custom_error_messages[r].message);
			var u = r.replace("[", "");
			u = (u = u.replace("]", "")).replace("custom", "");
			var c = e.data("message-" + u);
			return null != c && c.length > 0 && (s = c), s
		},
		_validityProp:
		{
			required: "value-missing",
			custom: "custom-error",
			groupRequired: "value-missing",
			ajax: "custom-error",
			minSize: "range-underflow",
			maxSize: "range-overflow",
			min: "range-underflow",
			max: "range-overflow",
			past: "type-mismatch",
			future: "type-mismatch",
			dateRange: "type-mismatch",
			dateTimeRange: "type-mismatch",
			maxCheckbox: "range-overflow",
			minCheckbox: "range-underflow",
			equals: "pattern-mismatch",
			funcCall: "custom-error",
			funcCallRequired: "custom-error",
			creditCard: "pattern-mismatch",
			condRequired: "value-missing"
		},
		_required: function (t, a, r, i, s) {
			switch (t.prop("type")) {
				case "radio":
				case "checkbox":
					if (s) {
						if (!t.prop("checked")) return i.allrules[a[r]].alertTextCheckboxMultiple;
						break
					}
					var o = t.closest("form, .validationEngineContainer"),
						n = t.attr("name");
					if (0 == o.find("input[name='" + n + "']:checked").size()) return 1 == o.find("input[name='" + n + "']:visible").size() ? i.allrules[a[r]].alertTextCheckboxe : i.allrules[a[r]].alertTextCheckboxMultiple;
					break;
				case "editor":
					if (t.hasClass("ext-control")) {
						var l = t.attr("id") + "_txtTemplate";
						if (e("#" + l).length > 0 && "undefined" != typeof tinyMCE) {
							var d = tinyMCE.get(l).getContent();
							if (0 === d.length || "<p></p>" === d) return i.allrules[a[r]].alertText
						}
					}
					break;
				case "text":
				case "password":
				case "textarea":
				case "file":
				case "select-one":
				case "select-multiple":
				default:
					var u = e.trim(t.val()),
						c = e.trim(t.attr("data-validation-placeholder")),
						f = e.trim(t.attr("placeholder"));
					if (!u || c && u == c || f && u == f) return i.allrules[a[r]].alertText
			}
		},
		_groupRequired: function (a, r, i, s) {
			var o = "[" + s.validateAttribute + "*=" + r[i + 1] + "]",
				n = !1;
			if (a.closest("form, .validationEngineContainer").find(o).each(function () {
				if (!t._required(e(this), r, i, s)) return n = !0, !1
			}), !n) return s.allrules[r[i]].alertText
		},
		_custom: function (e, t, a, r) {
			var i = t[a + 1],
				s = null,
				o = e.data("message-" + i),
				n = r.allrules[i];
			if ("extregex" === i) {
				var l = e.data("regex");
				null != l && l.length > 0 && (s = new RegExp(l))
			}
			else if (null != n && n.regex) {
				var d;
				if (null !== o && (o = r.allrules[i].alertText), !n) return void alert("jqv:custom rule not found - " + i);
				if (!(s = n.regex)) return void alert("jqv:custom regex not found - " + i)
			}
			if (null != s) {
				if (!new RegExp(s).test(e.val())) return o
			}
			else {
				if (void 0 === n || !n.func) return void alert("jqv:custom type not allowed " + i);
				if ("function" != typeof (d = n.func)) return void alert("jqv:custom parameter 'function' is no function - " + i);
				if (!d(e, t, a, r)) return o
			}
		},
		_funcCall: function (e, t, a, r) {
			var i, s = t[a + 1];
			if (s.indexOf(".") > -1) {
				for (var o = s.split("."), n = window; o.length;) n = n[o.shift()];
				i = n
			}
			else i = window[s] || r.customFunctions[s];
			if ("function" == typeof i) return i(e, t, a, r)
		},
		_funcCallRequired: function (e, a, r, i) {
			return t._funcCall(e, a, r, i)
		},
		_equals: function (t, a, r, i) {
			var s = a[r + 1];
			if (t.val() != e("#" + s).val()) return i.allrules.equals.alertText
		},
		_maxSize: function (e, t, a, r) {
			var i = t[a + 1];
			if (e.val().length > i) {
				var s = r.allrules.maxSize;
				return s.alertText + i + s.alertText2
			}
		},
		_minSize: function (e, t, a, r) {
			var i = t[a + 1];
			if (e.val().length < i) {
				var s = r.allrules.minSize;
				return s.alertText + i + s.alertText2
			}
		},
		_min: function (e, t, a, r) {
			var i = parseFloat(t[a + 1]);
			if (parseFloat(e.val()) < i) {
				var s = r.allrules.min;
				return s.alertText2 ? s.alertText + i + s.alertText2 : s.alertText + i
			}
		},
		_max: function (e, t, a, r) {
			var i = parseFloat(t[a + 1]);
			if (parseFloat(e.val()) > i) {
				var s = r.allrules.max;
				return s.alertText2 ? s.alertText + i + s.alertText2 : s.alertText + i
			}
		},
		_past: function (a, r, i, s, o) {
			var n, l = i[s + 1],
				d = e(a.find("*[name='" + l.replace(/^#+/, "") + "']"));
			if ("now" == l.toLowerCase()) n = new Date;
			else if (null != d.val()) {
				if (d.is(":disabled")) return;
				n = t._parseDate(d.val())
			}
			else n = t._parseDate(l);
			if (t._parseDate(r.val()) > n) {
				var u = o.allrules.past;
				return u.alertText2 ? u.alertText + t._dateToString(n) + u.alertText2 : u.alertText + t._dateToString(n)
			}
		},
		_future: function (a, r, i, s, o) {
			var n, l = i[s + 1],
				d = e(a.find("*[name='" + l.replace(/^#+/, "") + "']"));
			if ("now" == l.toLowerCase()) n = new Date;
			else if (null != d.val()) {
				if (d.is(":disabled")) return;
				n = t._parseDate(d.val())
			}
			else n = t._parseDate(l);
			if (t._parseDate(r.val()) < n) {
				var u = o.allrules.future;
				return u.alertText2 ? u.alertText + t._dateToString(n) + u.alertText2 : u.alertText + t._dateToString(n)
			}
		},
		_isDate: function (e) {
			return new RegExp(/^\d{4}[\/\-](0?[1-9]|1[012])[\/\-](0?[1-9]|[12][0-9]|3[01])$|^(?:(?:(?:0?[13578]|1[02])(\/|-)31)|(?:(?:0?[1,3-9]|1[0-2])(\/|-)(?:29|30)))(\/|-)(?:[1-9]\d\d\d|\d[1-9]\d\d|\d\d[1-9]\d|\d\d\d[1-9])$|^(?:(?:0?[1-9]|1[0-2])(\/|-)(?:0?[1-9]|1\d|2[0-8]))(\/|-)(?:[1-9]\d\d\d|\d[1-9]\d\d|\d\d[1-9]\d|\d\d\d[1-9])$|^(0?2(\/|-)29)(\/|-)(?:(?:0[48]00|[13579][26]00|[2468][048]00)|(?:\d\d)?(?:0[48]|[2468][048]|[13579][26]))$/).test(e)
		},
		_isDateTime: function (e) {
			return new RegExp(/^\d{4}[\/\-](0?[1-9]|1[012])[\/\-](0?[1-9]|[12][0-9]|3[01])\s+(1[012]|0?[1-9]){1}:(0?[1-5]|[0-6][0-9]){1}:(0?[0-6]|[0-6][0-9]){1}\s+(am|pm|AM|PM){1}$|^(?:(?:(?:0?[13578]|1[02])(\/|-)31)|(?:(?:0?[1,3-9]|1[0-2])(\/|-)(?:29|30)))(\/|-)(?:[1-9]\d\d\d|\d[1-9]\d\d|\d\d[1-9]\d|\d\d\d[1-9])$|^((1[012]|0?[1-9]){1}\/(0?[1-9]|[12][0-9]|3[01]){1}\/\d{2,4}\s+(1[012]|0?[1-9]){1}:(0?[1-5]|[0-6][0-9]){1}:(0?[0-6]|[0-6][0-9]){1}\s+(am|pm|AM|PM){1})$/).test(e)
		},
		_dateCompare: function (e, t) {
			return new Date(e.toString()) < new Date(t.toString())
		},
		_dateRange: function (e, a, r, i) {
			return !i.firstOfGroup[0].value && i.secondOfGroup[0].value || i.firstOfGroup[0].value && !i.secondOfGroup[0].value ? i.allrules[a[r]].alertText + i.allrules[a[r]].alertText2 : t._isDate(i.firstOfGroup[0].value) && t._isDate(i.secondOfGroup[0].value) && t._dateCompare(i.firstOfGroup[0].value, i.secondOfGroup[0].value) ? void 0 : i.allrules[a[r]].alertText + i.allrules[a[r]].alertText2
		},
		_dateTimeRange: function (e, a, r, i) {
			return !i.firstOfGroup[0].value && i.secondOfGroup[0].value || i.firstOfGroup[0].value && !i.secondOfGroup[0].value ? i.allrules[a[r]].alertText + i.allrules[a[r]].alertText2 : t._isDateTime(i.firstOfGroup[0].value) && t._isDateTime(i.secondOfGroup[0].value) && t._dateCompare(i.firstOfGroup[0].value, i.secondOfGroup[0].value) ? void 0 : i.allrules[a[r]].alertText + i.allrules[a[r]].alertText2
		},
		_maxCheckbox: function (e, t, a, r, i) {
			var s = a[r + 1],
				o = t.attr("name");
			if (e.find("input[name='" + o + "']:checked").size() > s) return i.showArrow = !1, i.allrules.maxCheckbox.alertText2 ? i.allrules.maxCheckbox.alertText + " " + s + " " + i.allrules.maxCheckbox.alertText2 : i.allrules.maxCheckbox.alertText
		},
		_minCheckbox: function (e, t, a, r, i) {
			var s = a[r + 1],
				o = t.attr("name");
			if (e.find("input[name='" + o + "']:checked").size() < s) return i.showArrow = !1, i.allrules.minCheckbox.alertText + " " + s + " " + i.allrules.minCheckbox.alertText2
		},
		_creditCard: function (e, t, a, r) {
			var i = !1,
				s = e.val().replace(/ +/g, "").replace(/-+/g, ""),
				o = s.length;
			if (o >= 14 && o <= 16 && parseInt(s) > 0) {
				var n, l = 0,
					d = (a = o - 1, 1),
					u = new String;
				do {
					n = parseInt(s.charAt(a)), u += d++ % 2 == 0 ? 2 * n : n
				} while (--a >= 0);
				for (a = 0; a < u.length; a++) l += parseInt(u.charAt(a));
				i = l % 10 == 0
			}
			if (!i) return r.allrules.creditCard.alertText
		},
		_ajax: function (a, r, i, s) {
			var o = r[i + 1],
				n = s.allrules[o],
				l = n.extraData,
				d = n.extraDataDynamic,
				u = {
					fieldId: a.attr("id"),
					fieldValue: a.val()
				};
			if ("object" == typeof l) e.extend(u, l);
			else if ("string" == typeof l) {
				var c = l.split("&");
				for (i = 0; i < c.length; i++) {
					var f = c[i].split("=");
					f[0] && f[0] && (u[f[0]] = f[1])
				}
			}
			if (d) {
				var v = String(d).split(",");
				for (i = 0; i < v.length; i++) {
					var p = v[i];
					if (e(p).length) {
						var m = a.closest("form, .validationEngineContainer").find(p).val();
						p.replace("#", ""), escape(m);
						u[p.replace("#", "")] = m
					}
				}
			}
			if ("field" == s.eventTrigger && delete s.ajaxValidCache[a.attr("id")], !s.isError && !t._checkAjaxFieldStatus(a.attr("id"), s)) return e.ajax(
				{
					type: s.ajaxFormValidationMethod,
					url: n.url,
					cache: !1,
					dataType: "json",
					data: u,
					field: a,
					rule: n,
					methods: t,
					options: s,
					beforeSend: function () { },
					error: function (e, a) {
						s.onFailure ? s.onFailure(e, a) : t._ajaxError(e, a)
					},
					success: function (r) {
						var i = r[0],
							o = e("#" + i).eq(0);
						if (1 == o.length) {
							var l = r[1],
								d = r[2];
							if (l) {
								if (s.ajaxValidCache[i] = !0, d) {
									if (s.allrules[d]) (u = s.allrules[d].alertTextOk) && (d = u)
								}
								else d = n.alertTextOk;
								s.showPrompts && (d ? t._showPrompt(o, d, "pass", !0, s) : t._closePrompt(o)), "submit" == s.eventTrigger && a.closest("form").submit()
							}
							else {
								var u;
								if (s.ajaxValidCache[i] = !1, s.isError = !0, d) {
									if (s.allrules[d]) (u = s.allrules[d].alertText) && (d = u)
								}
								else d = n.alertText;
								s.showPrompts && (d.trim().length > 0 && (d = "<li>" + d + "</li>"), t._showPrompt(o, d, "", !0, s))
							}
						}
						o.trigger("jqv.field.result", [o, s.isError, d])
					}
				}), n.alertTextLoad
		},
		_ajaxError: function (e, t) {
			0 == e.status && null == t ? alert("The page is not served from a server! ajax call failed") : "undefined" != typeof console && console.log("Ajax error: " + e.status + " " + t)
		},
		_dateToString: function (e) {
			return e.getFullYear() + "-" + (e.getMonth() + 1) + "-" + e.getDate()
		},
		_parseDate: function (e) {
			var t = e.split("-");
			return t == e && (t = e.split("/")), t == e ? (t = e.split("."), new Date(t[2], t[1] - 1, t[0])) : new Date(t[0], t[1] - 1, t[2])
		},
		_showPrompt: function (a, r, i, s, o, n) {
			a.data("jqv-prompt-at") instanceof jQuery ? a = a.data("jqv-prompt-at") : a.data("jqv-prompt-at") && (a = e(a.data("jqv-prompt-at")));
			var l = t._getPrompt(a);
			n && (l = !1), e.trim(r) && (l ? t._updatePrompt(a, l, r, i, s, o) : t._buildPrompt(a, r, i, s, o))
		},
		_buildPrompt: function (a, r, i, s, o) {
			if (0 === "inline".length) {
				var n = e("<div>");
				switch (n.addClass(t._getClassName(a.attr("id")) + "formError"), n.addClass("parentForm" + t._getClassName(a.closest("form, .validationEngineContainer").attr("id"))), n.addClass("formError"), i) {
					case "pass":
						n.addClass("greenPopup");
						break;
					case "load":
						n.addClass("blackPopup")
				}
				s && n.addClass("ajaxed");
				e("<div>").addClass("formErrorContent").html(r).appendTo(n);
				var l = a.data("promptPosition") || o.promptPosition;
				if (o.showArrow) {
					var d = e("<div>").addClass("formErrorArrow");
					if ("string" == typeof l) - 1 != (f = l.indexOf(":")) && (l = l.substring(0, f));
					switch (l) {
						case "bottomLeft":
						case "bottomRight":
							n.find(".formErrorContent").before(d), d.addClass("formErrorArrowBottom").html('<div class="line1"><!-- --></div><div class="line2"><!-- --></div><div class="line3"><!-- --></div><div class="line4"><!-- --></div><div class="line5"><!-- --></div><div class="line6"><!-- --></div><div class="line7"><!-- --></div><div class="line8"><!-- --></div><div class="line9"><!-- --></div><div class="line10"><!-- --></div>');
							break;
						case "topLeft":
						case "topRight":
							d.html('<div class="line10"><!-- --></div><div class="line9"><!-- --></div><div class="line8"><!-- --></div><div class="line7"><!-- --></div><div class="line6"><!-- --></div><div class="line5"><!-- --></div><div class="line4"><!-- --></div><div class="line3"><!-- --></div><div class="line2"><!-- --></div><div class="line1"><!-- --></div>'), n.append(d)
					}
				}
				o.addPromptClass && n.addClass(o.addPromptClass);
				var u = a.attr("data-required-class");
				if (void 0 !== u) n.addClass(u);
				else if (o.prettySelect && e("#" + a.attr("id")).next().is("select")) {
					var c = e("#" + a.attr("id").substr(o.usePrefix.length).substring(o.useSuffix.length)).attr("data-required-class");
					void 0 !== c && n.addClass(c)
				}
				n.css(
					{
						opacity: 0
					}), "inline" === l ? (n.addClass("inline"), void 0 !== a.attr("data-prompt-target") && e("#" + a.attr("data-prompt-target")).length > 0 ? n.appendTo(e("#" + a.attr("data-prompt-target"))) : a.after(n)) : a.before(n);
				var f = t._calculatePosition(a, n, o);
				return n.css(
					{
						position: "inline" === l ? "relative" : "absolute",
						top: f.callerTopPosition,
						left: f.callerleftPosition,
						marginTop: f.marginTopSize,
						opacity: 0
					}).data("callerField", a), o.autoHidePrompt && (setTimeout(function () {
						n.animate(
							{
								opacity: 0
							}, function () {
							n.closest(".formError").remove()
						})
					}, o.autoHideDelay), setTimeout(function () {
						n.animate(
							{
								opacity: 0
							}, function () {
							n.closest(".el-error").remove()
						})
					}, o.autoHideDelay)), n.animate(
						{
							opacity: .87
						})
			}
			var v = a;
			if ("SELECT" == v[0].tagName && ("select2" == v[0].classList[1] || "select2" == v[0].classList[2]))
				if (v.closest(".bootstrap-dialog-body").length > 0) setTimeout(function () {
					v.next("span").addClass("ext-error");
					var t = v.next("span").next();
					t.hasClass("el-error") && t.remove();
					//var a = e("<ul>").addClass("el-error").html(r),
					//	i = v.data("valid-target"),
					//	s = !1;
					//void 0 !== i && i.length > 0 && (void 0 !== (i = e(i)) && i.length > 0 && a.appendTo(i), s = !0), s || v.next("span").after(a)
				}, 500);
				else {
					v.next("span").addClass("ext-error"),
						(g = v.next("span").next()).hasClass("el-error") && g.remove();
					//var p = e("<ul>").addClass("el-error").html(r),
					//	m = !1;
					//void 0 !== (h = v.data("valid-target")) && h.length > 0
					//	&& (void 0 !== (h = e(h)) && h.length > 0 && p.appendTo(h), m = !0), m || v.next("span").after(p)
				}
			else if ("INPUT" == v[0].tagName)
				if (null != v[0].offsetParent)
					if ("input-group" == v[0].offsetParent.classList[0]) {
						v.closest(".input-group").addClass("ext-error"), (g = v.closest(".input-group").next()).hasClass("el-error") && g.remove();
						//p = e("<ul>").addClass("el-error").html(r), m = !1;
						//void 0 !== (h = v.data("valid-target")) && h.length > 0 && (void 0 !== (h = e(h)) && h.length > 0 && p.appendTo(h), m = !0), m || v.closest(".input-group").after(p)
					}
					else {
						v.addClass("ext-error"), (g = v.next()).hasClass("el-error") && g.remove();
						//p = e("<ul>").addClass("el-error").html(r), m = !1;
						//void 0 !== (h = v.data("valid-target")) && h.length > 0 && (void 0 !== (h = e(h)) && h.length > 0 && p.appendTo(h), m = !0), m || v.after(p)
					}
				else {
					v.addClass("ext-error"), (g = v.next()).hasClass("el-error") && g.remove();
					//p = e("<ul>").addClass("el-error").html(r), m = !1;
					//void 0 !== (h = v.data("valid-target")) && h.length > 0 && (void 0 !== (h = e(h)) && h.length > 0 && p.appendTo(h), m = !0), m || v.after(p)
				}
			else {
				var g;
				v.addClass("ext-error"), (g = v.next()).hasClass("el-error") && g.remove();
				//var h;
				//p = e("<ul>").addClass("el-error").html(r), m = !1;
				//void 0 !== (h = v.data("valid-target")) && h.length > 0 && (void 0 !== (h = e(h)) && h.length > 0 && p.appendTo(h), m = !0), m || v.after(p)
			}
			o.autoHidePrompt && (setTimeout(function () {
				n.animate(
					{
						opacity: 0
					}, function () {
					n.closest(".formError").remove()
				})
			}, o.autoHideDelay), setTimeout(function () {
				n.animate(
					{
						opacity: 0
					}, function () {
					n.closest(".el-error").remove()
				})
			}, o.autoHideDelay))
		},
		_updatePrompt: function (e, a, r, i, s, o, n) {
			if (a) {
				void 0 !== i && ("pass" == i ? a.addClass("greenPopup") : a.removeClass("greenPopup"), "load" == i ? a.addClass("blackPopup") : a.removeClass("blackPopup")), s ? a.addClass("ajaxed") : a.removeClass("ajaxed"), a.find(".formErrorContent").html(r);
				var l = t._calculatePosition(e, a, o),
					d = {
						top: l.callerTopPosition,
						left: l.callerleftPosition,
						marginTop: l.marginTopSize,
						opacity: .87
					};
				a.css(
					{
						opacity: 0,
						display: "block"
					}), n ? a.css(d) : a.animate(d)
			}
		},
		_closePrompt: function (e) {
			var a = e;
			if ("SELECT" == a[0].tagName && ("select2" == a[0].classList[1] || "select2" == a[0].classList[2])) a.next("span").removeClass("ext-error"), (r = a.next("span").next()).hasClass("el-error") && r.remove();
			else if ("INPUT" == a[0].tagName) {
				if (null != a[0].offsetParent && null != a[0].offsetParent)
					if ("input-group" == a[0].offsetParent.classList[0]) a.closest(".input-group").removeClass("ext-error"), (r = a.closest(".input-group").next()).hasClass("el-error") && r.remove();
					else a.removeClass("ext-error"), (r = a.next()).hasClass("el-error") && r.remove();
				else a.removeClass("ext-error"), (r = a.next()).hasClass("el-error") && r.remove()
			}
			else {
				var r;
				a.removeClass("ext-error"), (r = a.next()).hasClass("el-error") && r.remove()
			}
			var i = t._getPrompt(e);
			i && i.fadeTo("fast", 0, function () {
				i.closest(".formError").remove(), i.closest(".el-error").remove()
			})
		},
		closePrompt: function (e) {
			return t._closePrompt(e)
		},
		_getPrompt: function (a) {
			var r = e(a).closest("form, .validationEngineContainer").attr("id"),
				i = t._getClassName(a.attr("id")) + "formError",
				s = e("." + t._escapeExpression(i) + ".parentForm" + t._getClassName(r))[0];
			if (s) return e(s)
		},
		_escapeExpression: function (e) {
			return e.replace(/([#;&,\.\+\*\~':"\!\^$\[\]\(\)=>\|])/g, "\\$1")
		},
		isRTL: function (t) {
			var a = e(document),
				r = e("body"),
				i = t && t.hasClass("rtl") || t && "rtl" === (t.attr("dir") || "").toLowerCase() || a.hasClass("rtl") || "rtl" === (a.attr("dir") || "").toLowerCase() || r.hasClass("rtl") || "rtl" === (r.attr("dir") || "").toLowerCase();
			return Boolean(i)
		},
		_calculatePosition: function (e, t, a) {
			var r, i, s, o = e.width(),
				n = e.position().left,
				l = e.position().top;
			e.height();
			r = i = 0, s = -t.height();
			var d = e.data("promptPosition") || a.promptPosition,
				u = "",
				c = "",
				f = 0,
				v = 0;
			switch ("string" == typeof d && -1 != d.indexOf(":") && (u = d.substring(d.indexOf(":") + 1), d = d.substring(0, d.indexOf(":")), -1 != u.indexOf(",") && (c = u.substring(u.indexOf(",") + 1), u = u.substring(0, u.indexOf(",")), v = parseInt(c), isNaN(v) && (v = 0)), f = parseInt(u), isNaN(u) && (u = 0)), d) {
				default:
				case "topRight":
					i += n + o - 27, r += l;
					break;
				case "topLeft":
					r += l, i += n;
					break;
				case "centerRight":
					r = l + 4, s = 0, i = n + e.outerWidth(!0) + 5;
					break;
				case "centerLeft":
					i = n - (t.width() + 2), r = l + 4, s = 0;
					break;
				case "bottomLeft":
					r = l + e.height() + 5, s = 0, i = n;
					break;
				case "bottomRight":
					i = n + o - 27, r = l + e.height() + 5, s = 0;
					break;
				case "inline":
					i = 0, r = 0, s = 0
			}
			return {
				callerTopPosition: (r += v) + "px",
				callerleftPosition: (i += f) + "px",
				marginTopSize: s + "px"
			}
		},
		_saveOptions: function (t, a) {
			if (e.validationEngineLanguage) var r = e.validationEngineLanguage.allRules;
			else e.error("jQuery.validationEngine rules are not loaded, plz add localization files to the page");
			e.validationEngine.defaults.allrules = r;
			var i = e.extend(!0,
				{}, e.validationEngine.defaults, a);
			return t.data("jqv", i), i
		},
		_getClassName: function (e) {
			if (e) return e.replace(/:/g, "_").replace(/\./g, "_")
		},
		_jqSelector: function (e) {
			return e.replace(/([;&,\.\+\*\~':"\!\^#$%@\[\]\(\)=>\|])/g, "\\$1")
		},
		_condRequired: function (e, a, r, i) {
			var s, o;
			for (s = r + 1; s < a.length; s++)
				if ((o = jQuery("#" + a[s]).first()).length && null == t._required(o, ["required"], 0, i, !0)) return t._required(e, ["required"], 0, i)
		},
		_submitButtonClick: function (t) {
			var a = e(this);
			a.closest("form, .validationEngineContainer").data("jqv_submitButton", a.attr("id"))
		}
	};
	e.fn.validationEngine = function (a) {
		var r = e(this);
		return r[0] ? "string" == typeof a && "_" != a.charAt(0) && t[a] ? ("showPrompt" != a && "hide" != a && "hideAll" != a && t.init.apply(r), t[a].apply(r, Array.prototype.slice.call(arguments, 1))) : "object" != typeof a && a ? void e.error("Method " + a + " does not exist in jQuery.validationEngine") : (t.init.apply(r, arguments), t.attach.apply(r)) : r
	}, e.validationEngine = {
		fieldIdCounter: 0,
		defaults:
		{
			validationEventTrigger: "blur",
			scroll: !0,
			focusFirstField: !0,
			showPrompts: !0,
			validateNonVisibleFields: !1,
			ignoreFieldsWithClass: "ignoreMe",
			promptPosition: "topRight",
			bindMethod: "bind",
			inlineAjax: !1,
			ajaxFormValidation: !1,
			ajaxFormValidationURL: !1,
			ajaxFormValidationMethod: "get",
			onAjaxFormComplete: e.noop,
			onBeforeAjaxFormValidation: e.noop,
			onValidationComplete: !1,
			doNotShowAllErrosOnSubmit: !1,
			custom_error_messages:
				{},
			binded: !0,
			notEmpty: !1,
			showArrow: !0,
			showArrowOnRadioAndCheckbox: !1,
			isError: !1,
			maxErrorsPerField: !1,
			ajaxValidCache:
				{},
			autoPositionUpdate: !1,
			InvalidFields: [],
			onFieldSuccess: !1,
			onFieldFailure: !1,
			onSuccess: !1,
			onFailure: !1,
			validateAttribute: "class",
			addSuccessCssClassToField: "",
			addFailureCssClassToField: "",
			autoHidePrompt: !1,
			autoHideDelay: 5e3,
			fadeDuration: 300,
			prettySelect: !1,
			addPromptClass: "",
			usePrefix: "",
			useSuffix: "",
			showOneMessage: !1
		}
	}, e(function () {
		e.validationEngine.defaults.promptPosition = t.isRTL() ? "topLeft" : "topRight"
	})
}(jQuery);