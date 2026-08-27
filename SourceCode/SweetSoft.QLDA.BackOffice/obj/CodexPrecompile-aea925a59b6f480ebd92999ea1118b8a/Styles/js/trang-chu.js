class QuestionStartList {
    constructor(tableSelector) {
        this.dataBox = $(tableSelector);
        this.startGroup = this.dataBox.find('.detailStartGroup'); // chứa group
        this.detailStartQuestion = this.dataBox.find('.detailStartQuestion'); // Chứa danh sách con
        this.table = this.dataBox.find('table');
        this.tbody = this.table.find('tbody');
        this.bindEvents();
    }

    bindEvents() {
        this.detailStartQuestion.on('keypress', 'input', (e) => {
            if (e.which === 13) {
                e.preventDefault();
                const currenttbody = $(e.target).closest('tbody');
                this.createNewRow(currenttbody, $(e.target).closest('tr'));

            }
        });
        this.detailStartQuestion.on('click', '.btn-delete', (e) => {
            e.preventDefault();
            this.deleteRow($(e.target).closest('tr'), $(e.target).closest('tbody'));
        });

        this.detailStartQuestion.on('click', '.btn-question-upload', (e) => {
            e.preventDefault();
            const inputId = $(e.currentTarget).closest('.input-group').find('input').attr('id');
            this.OpenSelectFile(inputId);
        });

        //group
        this.startGroup.on('click', '.btn-save-class', (e) => {
            e.preventDefault();
            this.saveGroup();
        });

        this.startGroup.on('click', '.btn-cancel-class', (e) => {
            e.preventDefault();
            this.startGroup.find('[data-selector="startQuestion"]').val("");
            this.detailStartQuestion.find('.itemGroupDetailStartQuestion').removeClass('editingGroup');
            this.startGroup.find(".btn-cancel-class").addClass('hidden');
            this.startGroup.find(".btn-save-class").html('<i class="icon fas fa-plus"></i> Thêm');

        });

        this.detailStartQuestion.on('click', '.btn-edit-group', (e) => {
            e.preventDefault();
            this.editGroup($(e.target).closest('.itemGroupDetailStartQuestion'));
            this.startGroup.find(".btn-cancel-class").removeClass('hidden');
            this.startGroup.find(".btn-save-class").html('<i class="icon bx bxs-save"></i>Lưu');
        });

        this.detailStartQuestion.on('click', '.btn-delete-group', (e) => {
            e.preventDefault();
            this.deleteGroup($(e.target).closest('.itemGroupDetailStartQuestion'));
        });
    }


    OpenSelectFile(inputId) {
        var txtid = inputId;
        var w = $('.layout-content').width();
        var h = $('.layout-content').height();

        if (h > 800)
            h = 800;
        $.lightbox('/_RFMng/default.aspx?field_name=' + txtid
            + '&key=' + uploadThumbnailKey
            + '&selectFun=questionStartList.setImageUrl',
            {
                iframe: true,
                width: w - 60,
                height: h - 40
            });
    };
    setImageUrl(txtid, url, path) {
        var _urlBase = url.replaceAll(hostPath, '');
        const $thisElement = $('#' + txtid);
        const $currentRow = $thisElement.closest('.input-group');
        $thisElement.val(_urlBase);
        $currentRow.find('.btn-question-upload-view')
            .attr('href', url)
            .attr('target', '_blank');
    }


    deleteGroup(groupObj) {
        if (confirm('Bạn có chắc chắn muốn xóa nhóm này?')) {
            groupObj.remove();
        }
    }

    editGroup(objectSelector) {

        const question = objectSelector.find('[data-selector="itemGroupStartName"]').text();
        const itempoint = objectSelector.find('[data-selector="itemGroupStartPoint"]').text().trim();
        const itemtime = objectSelector.find('[data-selector="itemGroupStartTime"]').text().trim();

        const parent = this.startGroup;
        parent.find('[data-selector="startQuestion"]').val(question);
        parent.find('[data-selector="startTime"]').val(itemtime);
        parent.find('[data-selector="starMaxPoint"]').val(itempoint);
        // (optional) nếu muốn highlight row đang sửa
        this.detailStartQuestion.find('.itemGroupDetailStartQuestion').removeClass('editingGroup');
        objectSelector.addClass('editingGroup');
    }

    saveGroup() {
        const row = this.startGroup.find(".btn-save-class").closest('.row');
        const name = row.find('[data-selector="startQuestion"]').val();
        const maxpoint = row.find('[data-selector="starMaxPoint"]').val();
        const startTime = row.find('[data-selector="startTime"]').val();

        if (!name || !startTime || !maxpoint) {
            this.showToast('Vui lòng điền đầy đủ thông tin!');
            return;
        }

        const editingRow = this.detailStartQuestion.find('.editingGroup');
        if (editingRow.length) {

            editingRow.find('[data-selector="itemGroupStartName"]').text(name);
            editingRow.find('[data-selector="itemGroupStartPoint"]').text(maxpoint);
            editingRow.find('[data-selector="itemGroupStartTime"]').text(startTime);

            editingRow.removeClass('editingGroup');
            this.startGroup.find(".btn-cancel-class").addClass('hidden');
            this.startGroup.find(".btn-save-class").html('<i class="icon fas fa-plus"></i> Thêm');
        }
        else {
            const rowHtml = $(`
                     <div class="col-lg-12  mb-3 itemGroupDetailStartQuestion ">
                            <fieldset class="fieldset-box">
                                <legend class="text-primary fw-bold">
                                <span data-selector="itemGroupStartName">${name}</span>
                                <span>| Tổng điểm: <span data-selector="itemGroupStartPoint"> ${maxpoint}</span> điểm</span>
                                <span>| Thời gian: <span data-selector="itemGroupStartTime">${startTime}</span>s</span>

                                   <a title="Sửa" class="btn-grid-action text-decoration-underline text-warning me-2 btn-edit-group">
                                       <i class="fas fa-pencil-alt me-1"></i>
                                       <span>Sửa</span>
                                   </a>
                                    <a title="Xóa" class="btn-grid-action text-decoration-underline text-danger btn-delete-group">
                                        <i class="fas fa-trash me-1"></i>
                                        <span>Xóa</span>
                                    </a>
                                </legend>
                                <div class="row">
                                    <table class="table table-autocomplete w-100 table-bordered border-radius-fix table-hover" border="1" cellpadding="2">
                                        <thead>
                                            <tr>
                                                <th>STT</th>
                                                <th>Câu hỏi</th>
                                                <th>Đáp án</th>
                                                <th>File đính kèm</th>
                                                <th>Điểm đúng</th>
                                                <th class="hidden">Điểm sai</th>
                                                <th>Hành động</th>
                                            </tr>
                                        </thead>
                                        <tbody>
                                        <tr>
                                            <td class="text-center">1</td>
                                            <td style="width:47%">
                                                <textarea class="form-control ext-textbox start-question-input " placeholder="Câu hỏi"  rows="4"></textarea>
                                            </td>
                                            <td style="width:20%">
                                                <input type="text" class="form-control ext-textbox start-answer-input " placeholder="Đáp án" value="" autocomplete="off"></td>
                                            </td>  
                                            <td style="width: 15%;">
                                                    <div class="input-group mb-1">
                                                        <input type="text" id="${crypto.randomUUID()}"  placeholder="File đính kèm" data-selector="itemQuestionUpload" class="form-control" value="">
                                                        <button type="button" class="btn btn-info btn-question-upload" title="Thêm file đính kèm"><i class="fa fa-upload"></i></button>
                                                        <a href="javascript:void(0);" class="btn btn-info btn-question-upload-view"  title="Xem file đính kèm">
                                                            <i class="fa fa-eye icon"></i>
                                                        </a>
                                
                                                    </div>
                                                     <div class="input-group mb-1">
                                                    <input type="text" id="${crypto.randomUUID()}" data-selector="itemQuestionUpload2"  placeholder="File đính kèm" class="form-control" value="">
                                                    <button type="button" class="btn btn-info btn-question-upload" title="Thêm file đính kèm"><i class="fa fa-upload"></i></button>
                                                    <a href="#"
                                                       target="_blank"
                                                       class="btn btn-info btn-question-upload-view"
                                                       title="Xem file đính kèm">
                                                       <i class="fa fa-eye icon"></i>
                                                    </a>
                                                </div>
                                                <div class="input-group mb-1">
                                                    <input type="text" id="${crypto.randomUUID()}" data-selector="itemQuestionUpload3"  placeholder="File đính kèm" class="form-control" value="">
                                                    <button type="button" class="btn btn-info btn-question-upload" title="Thêm file đính kèm"><i class="fa fa-upload"></i></button>
                                                    <a href="#"
                                                       target="_blank"
                                                       class="btn btn-info btn-question-upload-view"
                                                       title="Xem file đính kèm">
                                                       <i class="fa fa-eye icon"></i>
                                                    </a>
                                                </div>
                                            </td>
                                            <td>
                                                <input type="number" class="form-control ext-textbox start-rightPoint-input " placeholder="Điểm đúng" value="10" autocomplete="off">
                                            </td>
                                            <td class="hidden">
                                                <input type="number" class="form-control ext-textbox start-wrongPoint-input " placeholder="Điểm sai" value="0" autocomplete="off">
                                            </td>
                                            <td class="" style="width:80px">
                                                <div class="d-flex justify-content-center">
                                                    <a title="Xóa" class="btn-grid-action text-decoration-underline text-danger btn-delete">
                                                        <i class="fas fa-trash me-1"></i>
                                                        <span>Xóa</span>
                                                    </a>
                                                </div>
                                            </td>
                                        </tr>
                                        </tbody>
                                    </table>
                                </div>
                            </fieldset>
                        </div>
            `);
            this.detailStartQuestion.append(rowHtml);
        }
        row.find('[data-selector="startQuestion"]').val("");
    }


    createNewRow(currenttbody, currentRow) {
        this.detailStartQuestion.find('tr').removeClass('duplicate');
        this.detailStartQuestion.find('input').removeClass('empty-field');
        this.detailStartQuestion.find('textarea').removeClass('empty-field');

        if (this.checkDuplicateQuestions()) {
            this.showToast(`Các câu hỏi hiện đang trùng nhau, vui lòng kiểm tra lại`, 'Thông báo');
            return;
        }
        if (this.isFullPoint(currenttbody)) {
            this.showToast(`Điểm thành phần đã lớn hơn hoặc bằng điểm tổng`, 'Thông báo');
            return;
        }
        if (!currenttbody) return;
        let oldRight = "10";
        let oldWrong = "0";

        if (currentRow) {
            oldRight = currentRow.find('td').eq(4).find('input').val().trim();
            oldWrong = currentRow.find('td').eq(5).find('input').val().trim();
        }
        const rowHtml = $(`
                <tr>
                    <td class="text-center">1</td>
                    <td style="width:47%">
                        <textarea class="form-control ext-textbox start-question-input " placeholder="Câu hỏi"  rows="4"> </textarea>
                    </td>
                    <td style="width:20%">
                        <input type="text" class="form-control ext-textbox start-answer-input " placeholder="Đáp án" value="" autocomplete="off"></td>
                    </td>
                      <td style="width: 15%;">
                            <div class="input-group mb-1">
                                <input type="text" id="${crypto.randomUUID()}" data-selector="itemQuestionUpload" placeholder="File đính kèm" class="form-control" value="">
                                <button type="button" class="btn btn-info btn-question-upload" title="File đính kèm"><i class="fa fa-upload"></i></button>
                                <a href="javascript:void(0);" class="btn btn-info btn-question-upload-view"  title="Xem file đính kèm">
                                    <i class="fa fa-eye icon"></i>
                                </a>
                            </div>
                              <div class="input-group mb-1">
                                <input type="text" id="${crypto.randomUUID()}" data-selector="itemQuestionUpload2" placeholder="File đính kèm" class="form-control" value="">
                                <button type="button" class="btn btn-info btn-question-upload" title="Thêm file đính kèm"><i class="fa fa-upload"></i></button>
                                <a href="javascript:void(0);" class="btn btn-info btn-question-upload-view"  title="Xem file đính kèm">
                                    <i class="fa fa-eye icon"></i>
                                </a>
                            </div>
                            <div class="input-group mb-1">
                                <input type="text" id="${crypto.randomUUID()}" data-selector="itemQuestionUpload3" placeholder="File đính kèm" class="form-control" value="">
                                <button type="button" class="btn btn-info btn-question-upload" title="Thêm file đính kèm"><i class="fa fa-upload"></i></button>
                                <a href="javascript:void(0);" class="btn btn-info btn-question-upload-view"  title="Xem file đính kèm">
                                    <i class="fa fa-eye icon"></i>
                                </a>
                            </div>
                        </td>
                    <td>

                        <input type="number" class="form-control ext-textbox start-rightPoint-input " placeholder="Điểm đúng" value="${oldRight}" autocomplete="off">
                    </td>
                    <td class="hidden">
                        <input type="number" class="form-control ext-textbox start-wrongPoint-input " placeholder="Điểm sai" value="${oldWrong}" autocomplete="off">
                    </td>
                    <td class="" style="width:70px">
                        <div class="d-flex justify-content-center">
                            <a title="Xóa" class="btn-grid-action text-decoration-underline text-danger btn-delete">
                                <i class="fas fa-trash me-1"></i>
                                <span>Xóa</span>
                            </a>
                        </div>
                    </td>
                </tr>
            `);
        currenttbody.append(rowHtml);
        this.reorderRows(currenttbody);
        //return row;
    }

    deleteRow(row, currentBody) {
        if (confirm('Bạn có chắc chắn muốn xóa câu hỏi này?')) {
            row.remove();
            this.reorderRows(currentBody);
        }
    }

    reorderRows(currenttbody) {
        currenttbody.find('tr').each((index, row) => {
            $(row).find('td:first').text(index + 1);
        });
        if (currenttbody.find('tr').length < 1)
            this.createNewRow(currenttbody, null);
    }

    // check valid
    isGroupDuplicated(name, currentObject) {
        if (!name) return false;
        let isDuplicated = false;
        this.detailStartQuestion.find('.itemGroupDetailStartQuestion').not(currentObject).each(function () {
            const rowData = $(this).find('[data-selector="itemGroupStartName"]').text().trim();
            if (rowData && rowData.trim() === name.trim()) {
                isDuplicated = true;
                return false;
            }
        });
        return isDuplicated;
    }

    // check valid
    isFullPoint(currentTbody, isExport = false) {
        let total = 0;
        //currentTbody.find('tr').find('td:eq(3)').removeClass('error-point');
        const $group = currentTbody.closest('.itemGroupDetailStartQuestion');
        const maxPoint = parseFloat($group.find('[data-selector="itemGroupStartPoint"]').text().trim()) || 0;

        currentTbody.find('tr').each(function () {
            const $td = $(this).find('td').eq(4);
            const value = parseFloat($td.find('input').val().trim()) || 0;
            total += value;
        });


        //currentTbody.find('tr').find('td:eq(3)').addClass('error-point');
        if (isExport) return total > maxPoint
        else return total >= maxPoint


        return false; // Không lỗi
    }
    checkEmptyQuestions() {
        let hasEmpty = false;
        // Duyệt qua từng nhóm câu hỏi
        this.detailStartQuestion.find('.itemGroupDetailStartQuestion').each((_, groupEl) => {
            const $group = $(groupEl);

            // Duyệt từng hàng trong nhóm
            $group.find('tbody tr').each((_, rowEl) => {
                const $row = $(rowEl);
                const $questionInput = $row.find('td').eq(1).find('textarea');
                const $answerInput = $row.find('td').eq(2).find('input');
                const $fileUploadInput = $row.find('td').eq(3).find('[data-selector="itemQuestionUpload"]');
                const $fileUploadInput2 = $row.find('td').eq(3).find('[data-selector="itemQuestionUpload2"]');
                const $fileUploadInput3 = $row.find('td').eq(3).find('[data-selector="itemQuestionUpload3"]');

                const question = ($questionInput.val() || '').trim();
                const answer = ($answerInput.val() || '').trim();
                const fileUpload = ($fileUploadInput.val() || '').trim();
                const fileUpload2 = ($fileUploadInput2.val() || '').trim();
                const fileUpload3 = ($fileUploadInput3.val() || '').trim();

                // Nếu thiếu câu hỏi hoặc đáp án → đánh dấu đỏ
                if (!question && !fileUpload && !fileUpload2 && !fileUpload3) {
                    $questionInput.addClass('empty-field');
                    hasEmpty = true;
                }
                if (!answer) {
                    $answerInput.addClass('empty-field');
                    hasEmpty = true;
                }
            });
        });

        return hasEmpty; // true nếu có ít nhất 1 lỗi, nhưng đã highlight hết
    }


    //checkDuplicateQuestions() {
    //    const seen = new Map();
    //    let hasDuplicate = false;

    //    this.detailStartQuestion.find('.itemGroupDetailStartQuestion').each((_, groupEl) => {
    //        if (hasDuplicate) return false;

    //        const $group = $(groupEl);

    //        $group.find('tbody tr').each((_, rowEl) => {
    //            if (hasDuplicate) return false;

    //            const $row = $(rowEl);
    //            const text = ($row.find('td').eq(1).find('textarea').val() || '').trim();
    //            if (!text) return;
    //            const key = text.toLowerCase();

    //            if (seen.has(key)) {
    //                seen.get(key).forEach(($r) => $r.addClass('duplicate'));
    //                $row.addClass('duplicate');
    //                hasDuplicate = true;
    //                return false;
    //            } else {
    //                seen.set(key, [$row]);
    //            }
    //        });
    //    });
    //    return hasDuplicate;
    //}
    checkDuplicateQuestions() {
        const seen = new Map();
        let hasDuplicate = false;

        this.detailStartQuestion.find('.itemGroupDetailStartQuestion').each((_, groupEl) => {
            if (hasDuplicate) return false;

            $(groupEl).find('tbody tr').each((_, rowEl) => {
                if (hasDuplicate) return false;

                const $row = $(rowEl);

                const question = ($row.find('td').eq(1).find('textarea').val() || '').trim().toLowerCase();
                if (!question) return;

                const answer = ($row.find('td').eq(2).find('input').val() || '').trim().toLowerCase();

                if (seen.has(question)) {
                    const storedRows = seen.get(question);

                    const matched = storedRows.find(stored => {
                        const storedAnswer = stored.answer;
                        return storedAnswer === answer;
                    });

                    if (matched) {
                        matched.row.addClass('duplicate');
                        $row.addClass('duplicate');
                        hasDuplicate = true;
                        return false;
                    } else {
                        storedRows.push({ row: $row, answer });
                    }

                } else {
                    seen.set(question, [{ row: $row, answer }]);
                }
            });
        });

        return hasDuplicate;
    }

    importFromJSON(jsonData) {

        if (!jsonData.isAllowAddUpdate)
            this.startGroup.addClass("hidden");
        if (!jsonData.ListGroup || !Array.isArray(jsonData.ListGroup)) {
            console.error('Invalid data');
            return;
        }


        jsonData.ListGroup.forEach((group, index) => {
            let html = '';
            if (jsonData.isAllowAddUpdate === true) {
                group.ListQuestion.forEach((questionDetail, index) => {
                    html += `
                    <tr>
                        <td class="text-center">${index + 1}</td>
                        <td style="width:47%">
                            <textarea class="form-control ext-textbox start-question-input " placeholder="Câu hỏi"  rows="4"> ${this.escapeHtml(questionDetail.question) || ''}</textarea>
                       </td >
                        <td style="width:20%">
                            <input type="text" class="form-control ext-textbox start-answer-input " placeholder="Đáp án" value="${this.escapeHtml(questionDetail.answer) || ''}" autocomplete="off"></td>
                        </td>
                          <td style="width: 15%;">
                            <div class="input-group mb-1">
                                <input type="text" id="${crypto.randomUUID()}" data-selector="itemQuestionUpload"  placeholder="File đính kèm" class="form-control" value="${questionDetail.questionFileUrl || ''}">
                                <button type="button" class="btn btn-info btn-question-upload"  title="File đính kèm"><i class="fa fa-upload"></i></button>
                                <a href="${questionDetail.questionFileUrl || 'javascript:void(0)'}" ${questionDetail.questionFileUrl ? 'target="_blank"' : ''}    class="btn btn-info btn-question-upload-view"  title="Xem file đính kèm">
                                    <i class="fa fa-eye icon"></i>
                                </a>
                            </div>
                              <div class="input-group mb-1">
                                <input type="text" id="${crypto.randomUUID()}" data-selector="itemQuestionUpload2"  placeholder="File đính kèm" class="form-control" value="${questionDetail.questionFileUrl2 || ''}">
                                <button type="button" class="btn btn-info btn-question-upload" title="Thêm file đính kèm"><i class="fa fa-upload"></i></button>
                                <a href="${questionDetail.questionFileUrl2 || 'javascript:void(0)'}" ${questionDetail.questionFileUrl2 ? 'target="_blank"' : ''}    class="btn btn-info btn-question-upload-view"  title="Xem file đính kèm">
                                    <i class="fa fa-eye icon"></i>
                                </a>
                            </div>
                            <div class="input-group mb-1">
                                <input type="text" id="${crypto.randomUUID()}" data-selector="itemQuestionUpload3"  placeholder="File đính kèm" class="form-control" value="${questionDetail.questionFileUrl3 || ''}">
                                <button type="button" class="btn btn-info btn-question-upload" title="Thêm file đính kèm"><i class="fa fa-upload"></i></button>
                                <a href="${questionDetail.questionFileUrl3 || 'javascript:void(0)'}" ${questionDetail.questionFileUrl3 ? 'target="_blank"' : ''}    class="btn btn-info btn-question-upload-view"  title="Xem file đính kèm">
                                    <i class="fa fa-eye icon"></i>
                                </a>
                            </div>
                        </td>
                        <td>
                            <input type="number" class="form-control ext-textbox start-rightPoint-input " placeholder="Điểm trả lời đúng" value="${questionDetail.rightPoint || ''}" autocomplete="off">
                       </td>
                        <td class="hidden">
                            <input type="number" class="form-control ext-textbox start-wrongPoint-input " placeholder="Điểm trả lời sai" value="${questionDetail.wrongPoint || ''}" autocomplete="off">
                        </td>
                        <td class="" style="width:80px">
                            <div class="d-flex justify-content-center">
                                <a title="Xóa" class="btn-grid-action text-decoration-underline text-danger btn-delete">
                                    <i class="fas fa-trash me-1"></i>
                                    <span>Xóa</span>
                                </a>
                            </div>
                        </td>
                </tr>
            `;
                });
            }
            else {
                group.ListQuestion.forEach((questionDetail, index) => {
                    html += `
                    <tr>
                        <td class="text-center">${index + 1}</td>
                        <td style="width:50%">
                           ${this.escapeHtml(questionDetail.question) || ''}
                       </td>
                        <td style="width:35%">
                           ${this.escapeHtml(questionDetail.answer) || ''}
                        </td>
                         <td style="width:10%">
                            <a href="${questionDetail.questionFileUrl || 'javascript:void(0)'}" target="_blank"    title="Xem file đính kèm" class="btn-grid-action text-decoration-underline text-primary ${questionDetail.questionFileUrl ? '' : "hidden"}">
                                <span>Xem</span> <i class="fa fa-eye icon"></i>
                            </a>
                               <a href="${questionDetail.questionFileUrl2 || 'javascript:void(0)'}" target="_blank"    title="Xem file đính kèm" class="btn-grid-action text-decoration-underline text-primary ${questionDetail.questionFileUrl ? '' : "hidden"}">
                                <span>Xem</span> <i class="fa fa-eye icon"></i>
                            </a> 
                            <a href="${questionDetail.questionFileUrl3 || 'javascript:void(0)'}" target="_blank"    title="Xem file đính kèm" class="btn-grid-action text-decoration-underline text-primary ${questionDetail.questionFileUrl ? '' : "hidden"}">
                                <span>Xem</span> <i class="fa fa-eye icon"></i>
                            </a>
                         </td>
                        <td>
                          ${questionDetail.rightPoint || ''}
                       </td>
                        <td class="hidden">
                           ${questionDetail.wrongPoint || ''}
                        </td>
                        <td class="hidden" style="width:80px">
                           
                        </td>
                </tr>
            `;
                });
            }

            const rowHtml = $(`
                 <div class="col-lg-12  mb-3 itemGroupDetailStartQuestion ">
                        <fieldset class="fieldset-box">
                            <legend class="text-primary fw-bold">
                            <span data-selector="itemGroupStartName">${group.itemGroupStartName}</span>
                            <span>| Tổng điểm: <span data-selector="itemGroupStartPoint"> ${group.itemGroupStartPoint}</span> điểm</span>
                            <span>| Thời gian: <span data-selector="itemGroupStartTime">${group.itemGroupStartTime}</span>s</span>
                               <a title="Sửa" class="btn-grid-action text-decoration-underline text-warning me-2 btn-edit-group ${jsonData.isAllowAddUpdate ? "" : "hidden"}">
                                   <i class="fas fa-pencil-alt me-1"></i>
                                   <span>Sửa</span>
                               </a>
                                <a title="Xóa" class="btn-grid-action text-decoration-underline text-danger btn-delete-group ${jsonData.isAllowAddUpdate ? "" : "hidden"}">
                                    <i class="fas fa-trash me-1"></i>
                                    <span>Xóa</span>
                                </a>
                            </legend>
                            <div class="row">
                                <table class="table table-autocomplete w-100 table-bordered border-radius-fix table-hover" border="1" cellpadding="2">
                                    <thead>
                                        <tr>
                                            <th>STT</th>
                                            <th>Câu hỏi</th>
                                            <th>Đáp án</th>
                                            <th>File đính kèm</th>
                                            <th>Điểm đúng</th>
                                            <th class="hidden">Điểm sai</th>
                                            <th class="${jsonData.isAllowAddUpdate ? "" : "hidden"}">Hành động</th>
                                        </tr>
                                    </thead>
                                    <tbody>
                                    ${html}
                                    </tbody>
                                </table>
                            </div>
                        </fieldset>
                    </div>
                `);
            this.detailStartQuestion.append(rowHtml);

        });
    }


    // export to json
    exportToJSON(isAllowAddUpdate = true) {

        this.detailStartQuestion.find('tr').removeClass('duplicate');
        this.detailStartQuestion.find('input').removeClass('empty-field');
        this.detailStartQuestion.find('textarea').removeClass('empty-field');

        if (this.checkDuplicateQuestions()) {
            this.showToast(`Các câu hỏi hiện đang trùng nhau, vui lòng kiểm tra lại`, 'Thông báo');
            return;
        }
        if (this.checkEmptyQuestions()) {
            this.showToast(`Vui lòng nhập đầy đủ câu hỏi và đáp án!`, 'Thông báo');
            return;
        }
        let isSuccess = true;
        const listGroup = [];

        this.detailStartQuestion.find('.itemGroupDetailStartQuestion').each((index, rowObj) => {
            if (!isSuccess) return;
            const $group = $(rowObj);
            const listQuestion = [];
            const itemGroupStartName = $group.find('[data-selector="itemGroupStartName"]').text();
            const itemGroupStartTime = parseInt($group.find('[data-selector="itemGroupStartTime"]').text().trim()) || 0;
            const itemGroupStartPoint = parseInt($group.find('[data-selector="itemGroupStartPoint"]').text().trim()) || 0;
            const $grouptbody = $group.find('tbody');

            if (this.isFullPoint($grouptbody, true)) {
                isSuccess = false;
                return;
            }

            $grouptbody.find('tr').each((index, row) => {
                const $row = $(row);
                var i = 1;
                listQuestion.push({
                    index: index + 1,
                    question: $row.find('td').eq(i++).find('textarea').val(),     // Câu hỏi
                    answer: $row.find('td').eq(i++).find('input').val(),       // Đáp án
                    questionFileUrl: $row.find('td').eq(i).find('[data-selector="itemQuestionUpload"]').val().trim(),
                    questionFileUrl2: $row.find('td').eq(i).find('[data-selector="itemQuestionUpload2"]').val().trim(),
                    questionFileUrl3: $row.find('td').eq(i++).find('[data-selector="itemQuestionUpload3"]').val().trim(),
                    rightPoint: parseInt($row.find('td').eq(i++).find('input').val().trim()) || 0, // Điểm đúng (int)
                    wrongPoint: parseInt($row.find('td').eq(i++).find('input').val().trim()) || 0, // Điểm sai (int)
                });
            });

            listGroup.push({
                index: index + 1,
                itemGroupStartName: itemGroupStartName,
                itemGroupStartTime: itemGroupStartTime,
                itemGroupStartPoint: itemGroupStartPoint,
                ListQuestion: listQuestion,
            });
        });

        if (!isSuccess) {
            this.showToast(`Điểm thành phần đã lớn hơn hoặc bằng điểm tổng`, 'Thông báo');
            return;
        }
        const result = {
            ListGroup: listGroup,
            totalItems: listGroup.length,
            isAllowAddUpdate: isAllowAddUpdate,
        };
        $('[data-selector="hdfDetailQuestionStart"]').val(JSON.stringify(result));
        $('[data-selector="btnSaveQuestionHidden"]')[0].click();
    }
    escapeHtml(str) {
        return (str || "")
            .replace(/&/g, "&amp;")
            .replace(/"/g, "&quot;")
            .replace(/'/g, "&#39;")
            .replace(/</g, "&lt;")
            .replace(/>/g, "&gt;");
    }
    showToast(message, title = '', type = 'warning') {
        if (!window.toastr) {
            alert(message);
            return;
        }
        switch (type) {
            case 'success':
                toastr.success(message, title, { timeOut: 3000, closeButton: true, progressBar: true });
                break;
            case 'error':
                toastr.error(message, title, { timeOut: 3000, closeButton: true, progressBar: true });
                break;
            case 'warning':
                toastr.warning(message, title, { timeOut: 3000, closeButton: true, progressBar: true });
                break;
            case 'info':
            default:
                toastr.info(message, title, { timeOut: 3000, closeButton: true, progressBar: true });
                break;
        }
        return true;
    }
    rebind() {
        //const startData = this.OvercomeObstacle;
        //if (startData) {
        //    const parsedData = startData;
        //    if (parsedData && startData.ListGroup) {
        //        this.importFromJSON(parsedData);
        //    } else {

        //        console.error('Invalid data');
        //    }
        //}

        const startData = $('[data-selector="hdfDetailQuestionStart"]').val();
        if (typeof startData === 'string' && startData.trim() !== '') {
            const parsedData = JSON.parse(startData);
            if (parsedData && parsedData.ListGroup) {
                this.importFromJSON(parsedData);
            }
            else
                console.error('Invalid data');
        }
        else
            console.warn('Invalid data');
    }
}


var questionStartList = new QuestionStartList('.detailQuestion');
