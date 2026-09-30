(function () {
    "use strict";

    var texts = window.dashboardResourceTexts || {};
    var activeEmployee = null;

    function formatText(template) {
        var values = Array.prototype.slice.call(arguments, 1);
        return String(template || "").replace(/\{(\d+)\}/g, function (_, index) {
            return values[Number(index)] == null ? "" : values[Number(index)];
        });
    }

    function appendText(parent, tagName, className, value) {
        var element = document.createElement(tagName);
        if (className) { element.className = className; }
        element.textContent = value || "";
        parent.appendChild(element);
        return element;
    }

    function renderLoadShares() {
        var donut = document.querySelector(".dashboard-resource .resource-load-donut");
        if (!donut) { return; }
        var values = (donut.getAttribute("data-resource-values") || "").split(",")
            .map(function (value) { return Number(value) || 0; });
        var total = values.reduce(function (sum, value) { return sum + value; }, 0);
        if (!total) { return; }
        var size = donut.clientWidth;
        var radius = size / 2;
        var colors = ["#36a778", "#efb63e", "#ef6d63"];
        var names = ["Rảnh", "Bình thường", "Quá tải"];
        var sweep = 0;
        var outside = [];
        donut.querySelectorAll(".resource-load-share, .resource-load-connector").forEach(function (node) {
            node.parentNode.removeChild(node);
        });
        values.forEach(function (value, index) {
            if (value <= 0) { return; }
            var degrees = value * 360 / total;
            var angle = (sweep + degrees / 2 - 90) * Math.PI / 180;
            var external = degrees < 14;
            var distance = external ? radius + 17 : radius - 11;
            var label = appendText(donut, "span",
                "resource-load-share" + (external ? " is-external" : ""),
                String(value));
            label.setAttribute("aria-hidden", "true");
            label.title = names[index] + ": " + value + " nhân sự";
            label.style.left = (radius + Math.cos(angle) * distance) + "px";
            label.style.top = (radius + Math.sin(angle) * distance) + "px";
            if (external) {
                outside.push({ label: label, angle: angle, x: radius + Math.cos(angle) * distance,
                    y: radius + Math.sin(angle) * distance, color: colors[index] });
            }
            sweep += degrees;
        });
        outside.forEach(function (item) {
            var line = document.createElement("span");
            line.className = "resource-load-connector";
            line.setAttribute("aria-hidden", "true");
            var startX = radius + Math.cos(item.angle) * (radius + 2);
            var startY = radius + Math.sin(item.angle) * (radius + 2);
            var dx = item.x - startX;
            var dy = item.y - startY;
            line.style.left = startX + "px";
            line.style.top = startY + "px";
            line.style.width = Math.sqrt(dx * dx + dy * dy) + "px";
            line.style.transform = "rotate(" + Math.atan2(dy, dx) + "rad)";
            line.style.backgroundColor = item.color;
            donut.appendChild(line);
        });
    }

    function bindLoadDonutInteractions() {
        var donut = document.querySelector(".dashboard-resource .resource-load-donut");
        if (!donut || donut.__resourceLoadInteractionsBound) { return; }
        donut.__resourceLoadInteractionsBound = true;

        var highlight = appendText(donut, "span", "resource-load-slice-highlight", "");
        highlight.setAttribute("aria-hidden", "true");
        var tooltip = appendText(donut, "span", "resource-load-slice-tooltip", "");
        tooltip.setAttribute("role", "tooltip");
        tooltip.setAttribute("aria-hidden", "true");

        function getSliceAt(event) {
            var rect = donut.getBoundingClientRect();
            var radius = Math.min(rect.width, rect.height) / 2;
            var x = event.clientX - rect.left - rect.width / 2;
            var y = event.clientY - rect.top - rect.height / 2;
            var distance = Math.sqrt(x * x + y * y);
            var padding = parseFloat(window.getComputedStyle(donut).paddingLeft) || 0;
            if (distance < radius - padding || distance > radius) { return -1; }

            var angle = (Math.atan2(y, x) * 180 / Math.PI + 90 + 360) % 360;
            var values = (donut.getAttribute("data-resource-values") || "").split(",")
                .map(function (value) { return Number(value) || 0; });
            var total = values.reduce(function (sum, value) { return sum + value; }, 0);
            if (!total) { return -1; }
            var start = 0;
            for (var index = 0; index < values.length; index++) {
                var sweep = values[index] * 360 / total;
                if (values[index] > 0 && angle >= start && angle < start + sweep) {
                    return { index: index, start: start, sweep: sweep, value: values[index] };
                }
                start += sweep;
            }
            return -1;
        }

        function clearHover() {
            highlight.classList.remove("is-visible");
            tooltip.classList.remove("is-visible");
            tooltip.setAttribute("aria-hidden", "true");
        }

        donut.addEventListener("mousemove", function (event) {
            var slice = getSliceAt(event);
            if (slice === -1) { clearHover(); return; }
            var values = (donut.getAttribute("data-resource-values") || "").split(",");
            var filter = ["free", "normal", "overloaded"][slice.index];
            var legendItem = document.querySelector(
                '.dashboard-resource .resource-load-legend-item[data-resource-filter="' + filter + '"]');
            var label = legendItem && legendItem.getAttribute("data-resource-title") || "";
            var rect = donut.getBoundingClientRect();
            highlight.style.setProperty("--resource-hover-start", slice.start + "deg");
            highlight.style.setProperty("--resource-hover-sweep", slice.sweep + "deg");
            highlight.classList.add("is-visible");
            tooltip.textContent = label + " · " + values[slice.index] + " nhân sự";
            tooltip.style.left = (event.clientX - rect.left) + "px";
            tooltip.style.top = (event.clientY - rect.top - 8) + "px";
            tooltip.classList.add("is-visible");
            tooltip.setAttribute("aria-hidden", "false");
        });
        donut.addEventListener("mouseleave", clearHover);
        donut.addEventListener("click", function (event) {
            var slice = getSliceAt(event);
            if (slice === -1) { return; }
            event.preventDefault();
            event.stopPropagation();
            var filter = ["free", "normal", "overloaded"][slice.index];
            var trigger = document.querySelector(
                '.dashboard-resource .resource-load-legend-item[data-resource-filter="' + filter + '"]');
            if (trigger) { trigger.click(); }
        });
    }

    function findEmployee(employeeId) {
        var employees = window.dashboardResourceDetailData || [];
        var normalizedId = String(employeeId || "").toLowerCase();
        var result = null;
        employees.some(function (employee) {
            if (String(employee.id || "").toLowerCase() === normalizedId) {
                result = employee;
                return true;
            }
            return false;
        });
        return result;
    }

    function findWeek(employee, weekStart) {
        if (!employee) { return null; }
        var result = null;
        (employee.weeks || []).some(function (week) {
            if (week.start === weekStart) {
                result = week;
                return true;
            }
            return false;
        });
        if (result) { return result; }
        (employee.months || []).some(function (month) {
            (month.weeks || []).some(function (week) {
                if (week.start === weekStart) {
                    result = week;
                    return true;
                }
                return false;
            });
            return !!result;
        });
        return result;
    }

    function findMonth(employee, monthStart) {
        if (!employee) { return null; }
        var result = null;
        (employee.months || []).some(function (month) {
            if (month.start === monthStart) {
                result = month;
                return true;
            }
            return false;
        });
        return result;
    }

    function findDay(employee, date) {
        if (!employee) { return null; }
        var result = null;
        (employee.weeks || []).some(function (week) {
            (week.days || []).some(function (day) {
                if (day.date === date) {
                    result = day;
                    return true;
                }
                return false;
            });
            return !!result;
        });
        if (result) { return result; }
        (employee.months || []).some(function (month) {
            (month.weeks || []).some(function (week) {
                (week.days || []).some(function (day) {
                    if (day.date === date) {
                        result = day;
                        return true;
                    }
                    return false;
                });
                return !!result;
            });
            return !!result;
        });
        return result;
    }

    function openDrawer() {
        var drawer = document.getElementById("resource-detail-drawer");
        if (!drawer || !window.bootstrap || !window.bootstrap.Modal) { return; }
        window.bootstrap.Modal.getOrCreateInstance(drawer).show();
    }

    function setDrawerHeading(employee, kicker, subtitle) {
        document.getElementById("resource-detail-title").textContent = employee.name;
        document.getElementById("resource-detail-subtitle").textContent = subtitle || "";
        document.getElementById("resource-detail-kicker").textContent = kicker || "";
        activeEmployee = employee;
    }

    function clearSummary() {
        var summary = document.getElementById("resource-detail-summary");
        var formula = document.getElementById("resource-detail-formula");
        summary.textContent = "";
        formula.textContent = "";
        return { summary: summary, formula: formula };
    }

    function appendBadge(parent, css, label) {
        return appendText(parent, "span", "resource-drawer-status " + (css || ""), label || "");
    }

    function renderTask(container, task) {
        var item = document.createElement(task.tasksUrl ? "a" : "div");
        item.className = "resource-drawer-task" + (task.tasksUrl ? " d-block text-decoration-none text-reset" : "");
        if (task.tasksUrl) { item.href = task.tasksUrl; }

        appendText(item, "div", "resource-drawer-task-project",
            (task.projectCode || texts.project || "") +
                (task.projectName ? " · " + task.projectName : ""));
        appendText(item, "div", "fw-semibold mt-1",
            (task.code || texts.task || "") + (task.name ? " - " + task.name : ""));
        if (task.startDate || task.endDate) {
            appendText(item, "div", "small text-muted mt-1",
                (task.startDate || "") + (task.endDate ? " – " + task.endDate : ""));
        }
        container.appendChild(item);
    }

    function renderDay(employee, day) {
        if (!employee || !day) { return; }
        var subtitle = (day.displayDate || day.date) +
            (employee.jobTitle ? " · " + employee.jobTitle : "") +
            (employee.department ? " · " + employee.department : "");
        setDrawerHeading(employee, texts.dailyDetail || texts.task || "", subtitle);

        var parts = clearSummary();
        appendBadge(parts.summary, "resource-drawer-status-primary " + (day.statusCss || ""), day.status);
        var summarySecondary = appendText(parts.summary, "div", "resource-drawer-summary-secondary", "");
        appendText(summarySecondary, "span", "resource-drawer-summary-type " + (day.dayTypeCss || ""), day.dayType);
        appendText(summarySecondary, "span", "resource-drawer-summary-count",
            formatText(texts.taskCountFormat, day.taskCount));
        appendText(parts.formula, "div", "small text-muted", texts.resourceLoadRules || "");

        var content = document.getElementById("resource-detail-days");
        content.textContent = "";
        appendText(content, "h6", "resource-drawer-section-title",
            formatText(texts.tasksForDate || "{0}", day.displayDate || day.date));
        if (!day.tasks || day.tasks.length === 0) {
            appendText(content, "div", "resource-drawer-empty", texts.noTasksOnDay || "");
        } else {
            day.tasks.forEach(function (task) { renderTask(content, task); });
        }
        openDrawer();
    }

    function buildDayButton(employee, day) {
        var button = document.createElement("button");
        button.type = "button";
        button.className = "resource-drawer-day-link";
        button.setAttribute("data-drawer-date", day.date);
        appendText(button, "strong", "", day.displayDate || day.date);
        appendBadge(button, "resource-drawer-status-primary " + (day.statusCss || ""), day.status);
        var secondary = appendText(button, "span", "resource-drawer-day-secondary", "");
        appendText(secondary, "span", "resource-drawer-day-type " + (day.dayTypeCss || ""), day.dayType);
        appendText(secondary, "span", "resource-drawer-day-count",
            formatText(texts.taskCountFormat, day.taskCount));
        return button;
    }

    function renderWeek(employee, week) {
        if (!employee || !week) { return; }
        setDrawerHeading(employee, texts.weekDetail || "",
            (week.label ? week.label + " · " : "") + (week.displayRange || ""));
        var parts = clearSummary();
        appendBadge(parts.summary, "resource-drawer-status-primary " + (week.statusCss || ""), week.status);
        appendText(parts.summary, "span", "resource-drawer-summary-count",
            formatText(texts.taskCountFormat, week.taskCount));
        appendText(parts.formula, "div", "small text-muted", texts.resourceLoadRules || "");

        var content = document.getElementById("resource-detail-days");
        content.textContent = "";
        appendText(content, "h6", "resource-drawer-section-title", texts.selectDay || "");
        (week.days || []).forEach(function (day) {
            content.appendChild(buildDayButton(employee, day));
        });
        openDrawer();
    }

    function renderMonth(employee, month) {
        if (!employee || !month) { return; }
        setDrawerHeading(employee, texts.monthSummary || "", month.label || "");
        var parts = clearSummary();
        appendBadge(parts.summary, "resource-drawer-status-primary " + (month.statusCss || ""), month.status);
        appendText(parts.summary, "span", "resource-drawer-month-counts",
            formatText(texts.monthDailyCounts,
                month.noLoadDayCount, month.normalDayCount, month.overloadedDayCount));
        appendText(parts.formula, "div", "small text-muted", texts.monthlyCalculation || "");

        var content = document.getElementById("resource-detail-days");
        content.textContent = "";
        appendText(content, "h6", "resource-drawer-section-title", texts.selectWeek || "");
        (month.weeks || []).forEach(function (week) {
            var button = document.createElement("button");
            button.type = "button";
            button.className = "resource-month-week-link resource-drawer-open-week";
            button.setAttribute("data-drawer-week", week.start);
            appendText(button, "strong", "", week.displayRange);
            appendText(button, "span", "resource-month-week-meta",
                formatText(texts.taskCountFormat, week.taskCount) + " · " + (week.status || ""));
            content.appendChild(button);
        });
        openDrawer();
    }

    function bindEmployeeQuickList() {
        var modal = document.getElementById("resourceEmployeesModal");
        var listBody = document.getElementById("resourceEmployeeListBody");
        var search = document.getElementById("resourceEmployeeSearch");
        var statusSelect = document.getElementById("resourceEmployeeStatus");
        var emptyRow = document.getElementById("resourceEmployeeSearchEmpty");
        var count = document.getElementById("resourceEmployeeListCount");
        if (!modal || !listBody || !search || !statusSelect) { return; }

        var rows = Array.prototype.slice.call(listBody.querySelectorAll("tr[data-resource-status]"));
        var selectedFilter = "all";
        var title = document.getElementById("resourceEmployeesModalTitle");
        var allTitle = document.querySelector(".dashboard-resource .resource-load-donut-center[data-resource-title]");
        allTitle = allTitle ? allTitle.getAttribute("data-resource-title") : "";

        function selectFilter(filter) {
            statusSelect.value = filter;
            if (!statusSelect.value) { statusSelect.value = "all"; }
            selectedFilter = statusSelect.value;
            if (title) {
                title.textContent = selectedFilter === "all" ? allTitle : statusSelect.options[statusSelect.selectedIndex].text;
            }
            applyFilter();
        }

        function applyFilter() {
            var query = (search.value || "").trim().toLocaleLowerCase();
            var visibleCount = 0;
            rows.forEach(function (row) {
                var status = row.getAttribute("data-resource-status");
                var matchesCategory = selectedFilter === "all" ||
                    (selectedFilter === "attention" && (status === "free" || status === "overloaded")) ||
                    status === selectedFilter;
                var matchesQuery = !query || row.textContent.toLocaleLowerCase().indexOf(query) >= 0;
                var visible = matchesCategory && matchesQuery;
                row.classList.toggle("d-none", !visible);
                if (visible) { visibleCount += 1; }
            });
            if (emptyRow && rows.length > 0) { emptyRow.classList.toggle("d-none", visibleCount > 0); }
            if (count) { count.textContent = visibleCount + " " + (texts.employeeLabel || ""); }
        }

        modal.addEventListener("show.bs.modal", function (event) {
            var trigger = event.relatedTarget;
            search.value = "";
            selectFilter(trigger ? trigger.getAttribute("data-resource-filter") || "all" : "all");
        });
        statusSelect.addEventListener("change", function () { selectFilter(statusSelect.value); });
        search.addEventListener("input", applyFilter);
        listBody.addEventListener("click", function (event) {
            var trigger = event.target.closest(".resource-list-open-week, .resource-list-open-month");
            if (!trigger) { return; }
            var employee = findEmployee(trigger.getAttribute("data-resource-person"));
            var week = findWeek(employee, trigger.getAttribute("data-resource-week"));
            var month = findMonth(employee, trigger.getAttribute("data-resource-month"));
            function openSelected() {
                if (month) { renderMonth(employee, month); }
                else { renderWeek(employee, week); }
            }
            var modalInstance = window.bootstrap && window.bootstrap.Modal
                ? window.bootstrap.Modal.getInstance(modal) : null;
            if (modalInstance) {
                modal.addEventListener("hidden.bs.modal", openSelected, { once: true });
                modalInstance.hide();
            } else { openSelected(); }
        });
    }

    function bindDrawer() {
        var dashboard = document.querySelector(".dashboard-resource");
        var drawer = document.getElementById("resource-detail-drawer");
        if (!dashboard) { return; }

        dashboard.addEventListener("click", function (event) {
            var target = event.target.closest(".resource-open-day, .resource-open-week, .resource-open-month");
            if (!target || !dashboard.contains(target)) { return; }
            var employee = findEmployee(target.getAttribute("data-resource-person"));
            if (target.classList.contains("resource-open-day")) {
                renderDay(employee, findDay(employee, target.getAttribute("data-resource-day")));
            } else if (target.classList.contains("resource-open-week")) {
                renderWeek(employee, findWeek(employee, target.getAttribute("data-resource-week")));
            } else {
                renderMonth(employee, findMonth(employee, target.getAttribute("data-resource-month")));
            }
        });

        if (drawer) {
            drawer.addEventListener("click", function (event) {
                var dayButton = event.target.closest("[data-drawer-date]");
                if (dayButton && activeEmployee) {
                    renderDay(activeEmployee, findDay(activeEmployee, dayButton.getAttribute("data-drawer-date")));
                    return;
                }
                var weekButton = event.target.closest("[data-drawer-week]");
                if (weekButton && activeEmployee) {
                    renderWeek(activeEmployee, findWeek(activeEmployee, weekButton.getAttribute("data-drawer-week")));
                }
            });
        }
    }

    function initialize() {
        renderLoadShares();
        bindLoadDonutInteractions();
        bindEmployeeQuickList();
        bindDrawer();
        var initial = window.dashboardResourceInitialDetail;
        if (initial && initial.person && initial.week) {
            renderWeek(findEmployee(initial.person), findWeek(findEmployee(initial.person), initial.week));
        }
        window.addEventListener("resize", renderLoadShares);
    }

    if (document.readyState === "loading") {
        document.addEventListener("DOMContentLoaded", initialize);
    } else {
        initialize();
    }
}());
