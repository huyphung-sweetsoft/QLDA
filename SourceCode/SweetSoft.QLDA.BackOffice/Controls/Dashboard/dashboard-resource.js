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

    function renderTrendChart() {
        var element = document.getElementById("resource-load-trend-chart");
        var data = window.dashboardResourceTrendData || [];

        if (!element || typeof ApexCharts === "undefined") {
            return;
        }

        var actual = [];
        var forecast = [];
        var firstForecastIndex = -1;

        data.forEach(function (item, index) {
            if (item.forecast) {
                if (firstForecastIndex < 0) {
                    firstForecastIndex = index;
                }
                actual.push(null);
                forecast.push(Number(item.utilization) || 0);
            } else {
                actual.push(Number(item.utilization) || 0);
                forecast.push(null);
            }
        });

        if (firstForecastIndex > 0) {
            forecast[firstForecastIndex - 1] = actual[firstForecastIndex - 1];
        }

        new ApexCharts(element, {
            chart: {
                type: "line",
                height: 320,
                toolbar: { show: false },
                zoom: { enabled: false }
            },
            series: [
                { name: texts.pastAssignment || "", data: actual },
                { name: texts.futurePlan || "", data: forecast }
            ],
            colors: ["#556ee6", "#f1b44c"],
            stroke: {
                curve: "smooth",
                width: [3, 3],
                dashArray: [0, 7]
            },
            markers: { size: 4, strokeWidth: 0 },
            dataLabels: { enabled: false },
            xaxis: {
                categories: data.map(function (item) { return item.label; })
            },
            yaxis: {
                min: 0,
                forceNiceScale: true,
                labels: { formatter: formatPercent },
                title: { text: texts.utilization || "" }
            },
            annotations: {
                yaxis: [{
                    y: 100,
                    borderColor: "#f46a6a",
                    strokeDashArray: 4,
                    label: {
                        borderColor: "#f46a6a",
                        style: { color: "#fff", background: "#f46a6a" },
                        text: texts.threshold100 || ""
                    }
                }]
            },
            legend: { position: "top", horizontalAlign: "right" },
            tooltip: {
                shared: true,
                intersect: false,
                x: {
                    formatter: function (value, options) {
                        var item = data[options.dataPointIndex];
                        return item
                            ? item.label + " (" + item.start + "–" + item.end + ")"
                            : value;
                    }
                },
                y: { formatter: formatPercent }
            }
        }).render();
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
        table.className = "table table-sm resource-drawer-day-table mb-0";
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
                "text-end text-nowrap resource-day-load",
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
        var load = document.getElementById("resource-detail-load");
        var capacity = document.getElementById("resource-detail-capacity");
        var formula = document.getElementById("resource-detail-formula");
        var weekStatus = document.getElementById("resource-detail-week-status");
        var dayContainer = document.getElementById("resource-detail-days");
        var projectContainer = document.getElementById("resource-detail-projects");
        var taskContainer = document.getElementById("resource-detail-tasks");
        var employee = findEmployee(employeeId);
        var week = employee ? findWeek(employee, weekStart) : null;

        if (!drawer || !backdrop || !employee || !week || !capacity ||
            !formula || !weekStatus || !dayContainer ||
            !projectContainer || !taskContainer) {
            return;
        }

        title.textContent = employee.name;
        subtitle.textContent = week.label + " · " + week.displayRange +
            (employee.jobTitle ? " · " + employee.jobTitle : "") +
            (employee.department ? " · " + employee.department : "");
        load.textContent = formatPercent(week.allocation);
        load.className = Number(week.allocation) > 100
            ? "text-danger"
            : Number(week.allocation) >= 80
                ? "text-warning"
                : Number(week.allocation) > 0
                    ? "text-success"
                    : "text-secondary";
        var days = week.days || [];
        var holidayDayCount = days.filter(function (day) {
            return day.isHoliday;
        }).length;
        var capacityText = formatDays(week.allocatedDays) + "/" +
            formatText(texts.capacityFormat, formatNumber(week.capacityDays)) +
            (Number(week.overAllocatedDays) > 0
                ? formatText(
                    texts.excessFormat,
                    formatDays(week.overAllocatedDays))
                : "") +
            (Number(week.overlapDayCount) > 0
                ? formatText(
                texts.overlapDaysFormat,
                    week.overlapDayCount)
                : "");
        if (holidayDayCount > 0) {
            capacityText += formatText(
                texts.holidayDaysFormat || "",
                holidayDayCount);
        }
        capacity.textContent = capacityText;
        formula.textContent = formatText(
            texts.formula || "",
            formatNumber(week.allocatedDays),
            formatNumber(week.capacityDays));
        renderWeekStatus(weekStatus, week, days);
        renderDailyAllocation(dayContainer, days);
        projectContainer.textContent = "";
        taskContainer.textContent = "";

        if (!week.projects || week.projects.length === 0) {
            appendText(
                projectContainer,
                "div",
                "resource-drawer-empty",
                texts.noProjectAllocation || "");
        } else {
            week.projects.forEach(function (project) {
                renderProject(projectContainer, project);
            });
        }

        if (!week.tasks || week.tasks.length === 0) {
            appendText(
                taskContainer,
                "div",
                "resource-drawer-empty",
                texts.noTasks || "");
        } else {
            week.tasks.forEach(function (task) {
                renderTask(taskContainer, task);
            });
        }

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

    function bindDrawer() {
        var dashboard = document.querySelector(".dashboard-resource");
        var closeButton = document.getElementById("resource-detail-close");
        var backdrop = document.getElementById("resource-detail-backdrop");

        if (!dashboard) {
            return;
        }

        dashboard.addEventListener("click", function (event) {
            var target = event.target;
            while (target && target !== dashboard &&
                !target.classList.contains("resource-load-button")) {
                target = target.parentElement;
            }

            if (target && target.classList.contains("resource-load-button")) {
                openDrawer(
                    target.getAttribute("data-resource-person"),
                    target.getAttribute("data-resource-week"));
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
        renderTrendChart();
        bindDrawer();
    }

    if (document.readyState === "loading") {
        document.addEventListener("DOMContentLoaded", initialize);
    } else {
        initialize();
    }
}());
