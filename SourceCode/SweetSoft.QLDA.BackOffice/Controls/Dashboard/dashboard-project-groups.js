(function () {
    "use strict";

    function formatCount(template, count) {
        return String(template || "{0}").replace(/\{0\}/g, count);
    }

    function create(body) {
        if (!body) { return null; }
        if (body.__dashboardProjectGroups) {
            return body.__dashboardProjectGroups;
        }

        var rows = Array.prototype.slice.call(
            body.querySelectorAll("tr[data-group-project-id]"));
        var groupsById = Object.create(null);
        var groups = [];

        rows.forEach(function (row) {
            var id = row.getAttribute("data-group-project-id") || "";
            if (!groupsById[id]) {
                groupsById[id] = {
                    code: row.getAttribute("data-group-project-code") || "",
                    name: row.getAttribute("data-group-project-name") || "",
                    rows: [],
                    expanded: false
                };
                groups.push(groupsById[id]);
            }
            groupsById[id].rows.push(row);
        });

        // A single project is already a project-level list; do not add a redundant toggle.
        if (groups.length < 2) { return null; }

        var table = body.closest("table");
        var columns = table ? table.querySelectorAll("thead tr:first-child th").length : 1;
        var countFormat = body.getAttribute("data-project-group-count-format") || "{0}";
        var fragment = document.createDocumentFragment();

        groups.forEach(function (group) {
            var header = document.createElement("tr");
            header.className = "dashboard-project-group-row";
            var cell = document.createElement("td");
            cell.colSpan = columns;
            var button = document.createElement("button");
            button.type = "button";
            button.className = "dashboard-project-group-toggle";
            button.setAttribute("aria-expanded", "false");

            var heading = document.createElement("span");
            heading.className = "dashboard-project-group-heading";
            var icon = document.createElement("i");
            icon.className = "bx bx-chevron-down";
            icon.setAttribute("aria-hidden", "true");
            heading.appendChild(icon);
            var code = document.createElement("strong");
            code.textContent = group.code || group.name;
            heading.appendChild(code);
            if (group.name && group.name !== group.code) {
                var name = document.createElement("span");
                name.className = "dashboard-project-group-name";
                name.textContent = group.name;
                heading.appendChild(name);
            }

            var count = document.createElement("span");
            count.className = "dashboard-project-group-count";
            button.appendChild(heading);
            button.appendChild(count);
            cell.appendChild(button);
            header.appendChild(cell);
            fragment.appendChild(header);
            group.header = header;
            group.button = button;
            group.count = count;

            group.rows.forEach(function (row) {
                row.classList.add("dashboard-project-group-collapsed");
                fragment.appendChild(row);
            });
            button.addEventListener("click", function () {
                group.expanded = !group.expanded;
                refresh();
            });
        });

        var emptyRow = body.querySelector("tr[data-search-empty]");
        body.insertBefore(fragment, emptyRow || null);

        function refresh(expandMatching) {
            groups.forEach(function (group) {
                var visibleCount = group.rows.filter(function (row) {
                    return !row.classList.contains("d-none");
                }).length;
                if (expandMatching && visibleCount > 0) {
                    group.expanded = true;
                }
                group.header.classList.toggle("d-none", visibleCount === 0);
                group.button.setAttribute("aria-expanded", group.expanded ? "true" : "false");
                group.count.textContent = formatCount(countFormat, visibleCount);
                group.rows.forEach(function (row) {
                    row.classList.toggle("dashboard-project-group-collapsed", !group.expanded);
                });
            });
        }

        function reset() {
            groups.forEach(function (group) { group.expanded = false; });
            refresh(false);
        }

        body.__dashboardProjectGroups = { refresh: refresh, reset: reset };
        refresh(false);
        return body.__dashboardProjectGroups;
    }

    window.DashboardProjectGroups = { create: create };
})();
