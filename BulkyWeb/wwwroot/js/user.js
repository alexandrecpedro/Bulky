﻿var dataTable;

$(function () {
    loadDataTable();
});

function loadDataTable() {
    dataTable = $('#tblData').DataTable({
        "ajax": { url: '/admin/user/getall' },
        "columns": [
            { data: 'name', "width": "15%" },
            { data: 'email', "width": "15%" },
            { data: 'phoneNumber', "width": "15%" },
            { data: 'company.name', "width": "15%" },
            { data: 'role', "width": "15%" },
            {
                data: { id: 'id', lockoutEnd: 'lockoutEnd' },
                "render": function (data) {
                    var today = new Date().getTime();
                    var lockout = new Date(data.lockoutEnd).getTime();

                    var isLocked = lockout > today;

                    var buttonClass = isLocked ? 'btn-danger' : 'btn-success';
                    var iconClass = isLocked ? 'bi-lock-fill' : 'bi-unlock-fill';
                    var lockStatus = isLocked ? 'Lock' : 'Unlock';

                    return `
                        <div class="text-center">
                            <a onclick=LockUnlock('${data.id}') class="btn ${buttonClass} text-white" style="cursor:pointer; width:100px;">
                                <i class="bi ${iconClass}"></i> ${lockStatus}
                            </a>
                            <a href="/admin/user/RoleManagement?userId=${data.id}" class="btn btn-danger text-white" style="width:150px;">
                                <i class="bi bi-pencil-square"></i> Permission
                            </a>
                        </div>
                    `
                },
                "width": "25%"
            }
        ]
    });
}

function LockUnlock(id) {
    $.ajax({
        type: "POST",
        url: '/Admin/User/LockUnlock',
        data: JSON.stringify(id),
        contentType: "application/json",
        success: function (data) {
            if (data.success) {
                toastr.success(data.message);
                dataTable.ajax.reload();
            }
        }
    });
}