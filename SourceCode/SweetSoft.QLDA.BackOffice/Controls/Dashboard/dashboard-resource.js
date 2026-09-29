(function () {
    "use strict";

    var texts = window.dashboardResourceTexts || {};

    function formatText(template) {
        var values = Array.prototype.slice.call(arguments, 1);
        return String(template || "").replace(/\{(\d+)\}/g, function (_, index) {
            return values[Number(index)] == null ? "" : values[Number(index)];
        });
    }

    function formatPercent(value) {
        var number = Number(value) || 0;
        return number.toFixed(1).replace(".0", "") + "%";
    }

    function appendText(parent, tagName, className, value) {
        var element = document.createElement(tagName);
        if (className) {
            element.className = className;
        }
        element.textContent = value || "";
        parent.appendChild(element);
        return element;
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
        var result = null;
        (employee.weeks || []).some(function (week) {
            if (week.start === weekStart) {
                result = week;
                return true;
            }
            return false;
        });
        return result;
    }

    function findMonth(employee, monthStart) {
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

    function loadStatus(value) {
        var percent = Number(value) || 0;
        if (percent <= 0) { return { text: texts.noLoad, css: "resource-load-none" }; }
        if (percent < 80) { return { text: texts.underloaded, css: "resource-load-low" }; }
        if (percent <= 100) { return { text: texts.balanced, css: "resource-load-balanced" }; }
        return { text: texts.overloaded, css: "resource-load-over" };
    }

    function renderSummary(container, allocation, allocatedDays, capacityDays, overrideStatus) {
        container.textContent = "";
        var status = overrideStatus || loadStatus(allocation);
        appendText(container, "strong", "resource-drawer-percent", formatPercent(allocation));
        appendText(container, "span", "resource-drawer-status " + status.css, status.text);
        appendText(container, "span", "resource-drawer-days", formatText(
            texts.weekDaysComparison, formatNumber(allocatedDays), formatNumber(capacityDays)));
    }

    function renderWeekScheduleNote(container, week) {
        if (!container || Number(week.capacityDays) === 5) { return; }

        var changes = [];
        (week.days || []).forEach(function (day) {
            var date = new Date(String(day.date || "") + "T00:00:00Z");
            if (isNaN(date.getTime())) { return; }
            var weekday = date.getUTCDay();
            var usualWorkday = weekday >= 1 && weekday <= 5;
            var label = day.displayDate || day.date;

            if (usualWorkday && !day.isWorkingDay) {
                if (day.isHoliday) {
                    changes.push(formatText(texts.weekExceptionOffNote, label,
                        day.holidayName ? " (" + day.holidayName + ")" : ""));
                } else {
                    changes.push(formatText(texts.weekScheduledOffNote, label));
                }
            } else if (!usualWorkday && day.isWorkingDay) {
                changes.push(formatText(texts.weekWeekendWorkNote, label));
            }
        });

        appendText(container, "div", "resource-drawer-calendar-note",
            changes.length
                ? formatText(texts.weekScheduleChange, changes.join("; "))
                : texts.weekScheduleChanged || "");
    }

    function formatDays(value) {
        var number = Number(value) || 0;
        return formatText(
            texts.dayFormat,
            number.toFixed(1).replace(".0", ""));
    }

    function formatNumber(value) {
        var number = Number(value) || 0;
        return number.toFixed(1).replace(".0", "");
    }

    function renderLoadFormula(container, allocation, allocatedDays, capacityDays, template) {
        if (!container) { return; }
        container.textContent = "";

        if (Number(capacityDays) <= 0) {
            appendText(container, "div", "fw-semibold", texts.noWorkingDaysLoad || "");
            return;
        }

        appendText(container, "div", "fw-semibold text-dark", formatText(
            template,
            formatNumber(allocatedDays),
            formatNumber(capacityDays),
            formatPercent(allocation)));
        appendText(container, "div", "small text-muted mt-1", texts.loadCalculationNote || "");
    }

    function renderProject(container, project) {
        var card = document.createElement(project.detailUrl ? "a" : "div");
        card.className = "resource-drawer-project" +
            (project.detailUrl ? " d-block text-decoration-none text-reset" : "");
        if (project.detailUrl) {
            card.href = project.detailUrl;
        }

        var heading = document.createElement("div");
        heading.className = "resource-drawer-project-heading";
        appendText(
            heading,
            "div",
            "fw-semibold",
            (project.code || texts.project || "") +
                (project.name ? " · " + project.name : ""));
        appendText(
            heading,
            "strong",
            Number(project.allocation) > 100
                ? "text-danger text-nowrap"
                : "text-primary text-nowrap",
            formatPercent(project.allocation));
        card.appendChild(heading);

        appendText(
            card,
            "div",
            "small text-muted mt-1",
            formatDays(project.allocatedDays) + " · " +
                formatText(
                    texts.taskCountFormat,
                    Number(project.taskCount) || 0));
        container.appendChild(card);
    }

    function renderTask(container, task) {
        var card = document.createElement(task.tasksUrl ? "a" : "div");
        card.className = "resource-drawer-task" +
            (task.tasksUrl ? " d-block text-decoration-none text-reset" : "");
        if (task.tasksUrl) {
            card.href = task.tasksUrl;
        }

        appendText(
            card,
            "div",
            "resource-drawer-task-project",
            (task.projectCode || texts.project || "") +
                (task.projectName ? " · " + task.projectName : ""));
        appendText(
            card,
            "div",
            "fw-semibold mt-1",
            (task.code || texts.task || "") +
                (task.name ? " - " + task.name : ""));

        var meta = document.createElement("div");
        meta.className = "resource-drawer-task-meta";
        appendText(
            meta,
            "span",
            "",
            formatDays(task.allocatedDays) + " · " +
                (task.activeDates || []).join(", "));
        appendText(
            meta,
            "strong",
            "text-primary text-nowrap",
            formatPercent(task.allocation));
        card.appendChild(meta);
        container.appendChild(card);
    }

    function renderDailyTask(container, task) {
        var item = document.createElement(task.tasksUrl ? "a" : "div");
        item.className = "resource-day-task" +
            (task.tasksUrl ? " d-block text-decoration-none text-reset" : "");
        if (task.tasksUrl) {
            item.href = task.tasksUrl;
        }

        appendText(
            item,
            "div",
            "fw-semibold",
            (task.code || texts.task || "") +
                (task.name ? " - " + task.name : ""));
        appendText(
            item,
            "div",
            "small text-muted",
            (task.projectCode || texts.project || "") +
                (task.projectName ? " · " + task.projectName : ""));
        container.appendChild(item);
    }

    function getDayStatus(day) {
        if (day.isHoliday) {
            return {
                text: formatText(
                    texts.holidayDay,
                    day.holidayName || texts.nonWorkingDay || ""),
                css: "resource-day-holiday"
            };
        }

        if (!day.isWorkingDay) {
            return {
                text: texts.nonWorkingDay || "",
                css: "resource-day-non-working"
            };
        }

        return {
            text: texts.workingDay || "",
            css: "resource-day-working"
        };
    }

    function renderDailyAllocation(container, days) {
        container.textContent = "";
        days = days || [];
        if (days.length === 0) {
            appendText(
                container,
                "div",
                "resource-drawer-empty",
                texts.noTasks || "");
            return;
        }

        var wrapper = document.createElement("div");
        wrapper.className = "table-responsive";
        var table = document.createElement("table");
        table.className = "table dashboard-data-table table-bordered table-sm resource-drawer-day-table mb-0";
        var head = document.createElement("thead");
        var headRow = document.createElement("tr");
        [texts.date || "Date", texts.status || "Status",
            texts.task || "Task", texts.utilization || "Utilization"]
            .forEach(function (label) {
                appendText(headRow, "th", "", label);
            });
        head.appendChild(headRow);
        table.appendChild(head);

        var body = document.createElement("tbody");
        days.forEach(function (day) {
            var status = getDayStatus(day);
            var row = document.createElement("tr");
            if (day.isHoliday) {
                row.className = "resource-day-holiday-row";
            } else if (!day.isWorkingDay) {
                row.className = "resource-day-non-working-row";
            }

            appendText(row, "td", "resource-day-date", day.displayDate || day.date);
            appendText(row, "td", "resource-day-status " + status.css, status.text);

            var taskCell = document.createElement("td");
            if (!day.tasks || day.tasks.length === 0) {
                appendText(
                    taskCell,
                    "span",
                    "small text-muted",
                    texts.noTasksOnDay || texts.noTasks || "");
            } else {
                day.tasks.forEach(function (task) {
                    renderDailyTask(taskCell, task);
                });
            }
            row.appendChild(taskCell);

            appendText(
                row,
                "td",
                "text-center text-nowrap resource-day-load",
                formatPercent(day.allocation));
            body.appendChild(row);
        });

        table.appendChild(body);
        wrapper.appendChild(table);
        container.appendChild(wrapper);
    }

    function renderWeekStatus(container, week, days) {
        container.textContent = "";
        days = days || [];
        var workingDays = days.filter(function (day) {
            return day.isWorkingDay;
        });

        if (workingDays.length > 0 && Number(week.allocatedDays) <= 0) {
            appendText(
                container,
                "span",
                "badge resource-week-status resource-week-no-assignment",
                texts.noAssignmentWeek || "");
        }
    }

    function openDrawer(employeeId, weekStart) {
        var drawer = document.getElementById("resource-detail-drawer");
        var backdrop = document.getElementById("resource-detail-backdrop");
        var title = document.getElementById("resource-detail-title");
        var subtitle = document.getElementById("resource-detail-subtitle");
        var dayContainer = document.getElementById("resource-detail-days");
        var kicker = document.getElementById("resource-detail-kicker");
        var summary = document.getElementById("resource-detail-summary");
        var formula = document.getElementById("resource-detail-formula");
        var employee = findEmployee(employeeId);
        var week = employee ? findWeek(employee, weekStart) : null;

        if (!drawer || !backdrop || !title || !subtitle ||
            !employee || !week || !dayContainer) {
            return;
        }

        title.textContent = employee.name;
        subtitle.textContent = week.label + " · " + week.displayRange +
            (employee.jobTitle ? " · " + employee.jobTitle : "") +
            (employee.department ? " · " + employee.department : "");
        if (kicker) { kicker.textContent = texts.weekDetail || ""; }
        if (summary) {
            renderSummary(summary, week.allocation, week.allocatedDays, week.capacityDays);
            renderWeekScheduleNote(summary, week);
        }
        renderLoadFormula(formula, week.allocation, week.allocatedDays,
            week.capacityDays, texts.formula);
        renderDailyAllocation(dayContainer, week.days || []);

        showDrawer(drawer, backdrop);
    }

    function openMonthDrawer(employeeId, monthStart) {
        var drawer = document.getElementById("resource-detail-drawer");
        var backdrop = document.getElementById("resource-detail-backdrop");
        var title = document.getElementById("resource-detail-title");
        var subtitle = document.getElementById("resource-detail-subtitle");
        var kicker = document.getElementById("resource-detail-kicker");
        var summary = document.getElementById("resource-detail-summary");
        var formula = document.getElementById("resource-detail-formula");
        var content = document.getElementById("resource-detail-days");
        var employee = findEmployee(employeeId);
        var month = employee ? findMonth(employee, monthStart) : null;
        if (!drawer || !backdrop || !title || !subtitle || !content || !month) { return; }

        title.textContent = employee.name;
        subtitle.textContent = month.label +
            (employee.jobTitle ? " · " + employee.jobTitle : "") +
            (employee.department ? " · " + employee.department : "");
        if (kicker) { kicker.textContent = texts.monthSummary || ""; }
        if (summary) {
            renderSummary(summary, month.allocation, month.allocatedDays, month.capacityDays,
                { text: month.status, css: month.hasOverload ? "resource-load-over" : loadStatus(month.allocation).css });
        }
        renderLoadFormula(formula, month.allocation, month.allocatedDays,
            month.capacityDays, texts.monthFormula);
        content.textContent = "";
        appendText(content, "h6", "mb-2", texts.selectWeek || "");
        (month.weeks || []).forEach(function (week) {
            var link = document.createElement("a");
            link.className = "resource-month-week-link";
            link.href = week.detailUrl || "#";
            var left = document.createElement("span");
            left.className = "resource-month-week-info";
            appendText(left, "strong", "resource-month-week-range", week.displayRange);
            appendText(left, "small", "resource-month-week-meta", formatText(
                texts.weekDaysComparison,
                formatNumber(week.allocatedDays),
                formatNumber(week.capacityDays)) + " · " + formatText(
                    texts.taskCountFormat, week.taskCount));
            link.appendChild(left);
            var right = document.createElement("span");
            right.className = "resource-month-week-load";
            appendText(right, "strong", "", formatPercent(week.allocation));
            appendText(right, "small", "resource-drawer-status " +
                (week.statusCss || ""), week.status || "");
            appendText(right, "small", "resource-month-week-action", texts.viewWeek || "");
            link.appendChild(right);
            content.appendChild(link);
        });
        showDrawer(drawer, backdrop);
    }

    function showDrawer(drawer, backdrop) {

        backdrop.hidden = false;
        window.setTimeout(function () {
            backdrop.classList.add("is-open");
            drawer.classList.add("is-open");
            drawer.setAttribute("aria-hidden", "false");
        }, 0);
        document.body.classList.add("resource-drawer-open");
    }

    function closeDrawer() {
        var drawer = document.getElementById("resource-detail-drawer");
        var backdrop = document.getElementById("resource-detail-backdrop");
        if (!drawer || !backdrop) {
            return;
        }

        drawer.classList.remove("is-open");
        backdrop.classList.remove("is-open");
        drawer.setAttribute("aria-hidden", "true");
        document.body.classList.remove("resource-drawer-open");
        window.setTimeout(function () {
            if (!backdrop.classList.contains("is-open")) {
                backdrop.hidden = true;
            }
        }, 200);
    }

    function bindEmployeeQuickList() {
        var modal = document.getElementById("resourceEmployeesModal");
        var listBody = document.getElementById("resourceEmployeeListBody");
        var search = document.getElementById("resourceEmployeeSearch");
        var emptyRow = document.getElementById("resourceEmployeeSearchEmpty");
        var count = document.getElementById("resourceEmployeeListCount");

        if (!modal || !listBody || !search) {
            return;
        }

        var rows = Array.prototype.slice.call(
            listBody.querySelectorAll("tr[data-resource-status]")
        );
        var selectedFilter = "all";

        function applyFilter() {
            var query = (search.value || "")
                .trim()
                .toLocaleLowerCase();
            var visibleCount = 0;

            rows.forEach(function (row) {
                var status = row.getAttribute("data-resource-status");
                var matchesCategory = selectedFilter === "all"
                    || (selectedFilter === "attention" && status !== "balanced")
                    || status === selectedFilter;
                var matchesQuery = !query || row.textContent
                    .toLocaleLowerCase()
                    .indexOf(query) >= 0;
                var visible = matchesCategory && matchesQuery;

                row.classList.toggle("d-none", !visible);
                if (visible) {
                    visibleCount += 1;
                }
            });

            if (emptyRow && rows.length > 0) {
                emptyRow.classList.toggle("d-none", visibleCount > 0);
            }
            if (count) {
                count.textContent = visibleCount + " " + (texts.employeeLabel || "");
            }
        }

        modal.addEventListener("show.bs.modal", function (event) {
            var trigger = event.relatedTarget;
            var title = document.getElementById(
                "resourceEmployeesModalTitle");

            selectedFilter = trigger
                ? trigger.getAttribute("data-resource-filter") || "all"
                : "all";
            if (trigger && title) {
                title.textContent = trigger.getAttribute(
                    "data-resource-title") || title.textContent;
            }

            search.value = "";
            applyFilter();
        });

        search.addEventListener("input", applyFilter);
        listBody.addEventListener("click", function (event) {
            var trigger = event.target.closest(
                ".resource-list-open-week, .resource-list-open-month");
            if (!trigger) { return; }
            var employeeId = trigger.getAttribute("data-resource-person");
            var weekStart = trigger.getAttribute("data-resource-week");
            var monthStart = trigger.getAttribute("data-resource-month");
            function openSelected() {
                if (monthStart) { openMonthDrawer(employeeId, monthStart); }
                else { openDrawer(employeeId, weekStart); }
            }
            var modalInstance = window.bootstrap && window.bootstrap.Modal
                ? window.bootstrap.Modal.getInstance(modal) : null;
            if (modalInstance) {
                modal.addEventListener("hidden.bs.modal", openSelected,
                    { once: true });
                modalInstance.hide();
            } else {
                openSelected();
            }
        });
    }

    function bindDrawer() {
        var dashboard = document.querySelector(".dashboard-resource");
        var closeButton = document.getElementById("resource-detail-close");
        var backdrop = document.getElementById("resource-detail-backdrop");

        if (!dashboard) {
            return;
        }

        dashboard.addEventListener("click", function (event) {
            var target = event.target;
            target = target.closest(".resource-open-week, .resource-open-month");
            if (!target || !dashboard.contains(target)) { return; }
            if (target.classList.contains("resource-open-week")) {
                openDrawer(
                    target.getAttribute("data-resource-person"),
                    target.getAttribute("data-resource-week"));
            } else {
                openMonthDrawer(
                    target.getAttribute("data-resource-person"),
                    target.getAttribute("data-resource-month"));
            }
        });

        if (closeButton) {
            closeButton.addEventListener("click", closeDrawer);
        }
        if (backdrop) {
            backdrop.addEventListener("click", closeDrawer);
        }
        document.addEventListener("keydown", function (event) {
            if (event.key === "Escape") {
                closeDrawer();
            }
        });
    }

    function initialize() {
        bindEmployeeQuickList();
        bindDrawer();
        var initial = window.dashboardResourceInitialDetail;
        if (initial && initial.person && initial.week) {
            openDrawer(initial.person, initial.week);
        }
    }

    if (document.readyState === "loading") {
        document.addEventListener("DOMContentLoaded", initialize);
    } else {
        initialize();
    }
}());
