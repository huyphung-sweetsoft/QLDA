/* KhaiBaoGio 2.0 - nhap tong gio: phut, khong phai gio trong ngay.
   0600 -> 06:00, 630 -> 06:30, 12530 -> 125:30, 125:30 -> 125:30.
   Chuan nhap moi: 0..599,999 phut (0..9,999 gio 59 phut). Khong phu thuoc Date/time input 24h. */
(function (root) {
    'use strict';
    var LIMIT = 599999;
    function canonical(raw) {
        var s=String(raw==null?'':raw).trim();
        if (!s || !/^[0-9:]+$/.test(s)) return null;
        var h,m;
        if (s.indexOf(':')>=0) {
            if (!/^\d{1,7}:[0-5]\d$/.test(s)) return null;
            var parts=s.split(':');h=Number(parts[0]);m=Number(parts[1]);
        } else {
            if (!/^\d{1,9}$/.test(s)) return null;
            // Mot/hai chu so => tong so gio. 3+ chu so => hai so cuoi la phut.
            h=Number(s.length<=2?s:s.slice(0,-2));m=Number(s.length<=2?'0':s.slice(-2));
        }
        if (!Number.isSafeInteger(h)||!Number.isSafeInteger(m)||m>59||h*60+m>LIMIT) return null;
        return h.toString().padStart(2,'0')+':'+String(m).padStart(2,'0');
    }
    function minutes(raw) {
        var s=canonical(raw);if(s===null)return null;
        var p=s.split(':');return Number(p[0])*60+Number(p[1]);
    }
    function bind(input) {
        if (!input||input.dataset.kbgHourBound==='1')return;
        input.dataset.kbgHourBound='1';input.type='text';input.inputMode='numeric';
        input.autocomplete='off';input.spellcheck=false;input.maxLength=10;
        input.placeholder='Ví dụ: 06:30 / 125:30';
        input.setAttribute('title','Gõ 0600 = 06:00, 0630 = 06:30, 12530 = 125:30.');
        input.addEventListener('beforeinput',function(e){
            if(e.data && /[^0-9:]/.test(e.data))e.preventDefault();
        });
        input.addEventListener('input',function(){
            // paste/browser without beforeinput: reject invalid char; do not silently turn alphabet into 0h.
            if(/[^0-9:]/.test(input.value)){
                input.dataset.kbgHourInvalid='1'; input.setCustomValidity('Chỉ được nhập chữ số và dấu hai chấm.');
            } else {input.dataset.kbgHourInvalid='0';input.setCustomValidity('');}
        });
        input.addEventListener('blur',function(){
            if(input.dataset.kbgHourInvalid==='1')return;
            var c=canonical(input.value);
            if(c!==null){input.value=c;input.setCustomValidity('');input.classList.remove('is-invalid');}
            else if(input.value.trim()){input.classList.add('is-invalid');input.setCustomValidity('Giờ phải là HH:mm, phút 00-59; ví dụ 0600 hoặc 125:30.');}
        });
        input.addEventListener('focus',function(){input.classList.remove('is-invalid');input.setCustomValidity('');});
    }
    function validate(input) {
        if(!input||input.dataset.kbgHourInvalid==='1')return null;
        var c=canonical(input.value);if(c===null){if(input)input.classList.add('is-invalid');return null;}
        input.value=c;input.classList.remove('is-invalid');return minutes(c);
    }
    // DOT 8 HOTFIX: O nhap GIO va PHUT rieng; khong suy dien tu "67000" thanh mot HH:mm.
    // Gia tri 125 gio + 30 phut duoc phep: day la TONG gio lam, khong phai gio trong ngay.
    function fromTotal(rawMinutes) {
        var n=Number(rawMinutes);
        if(!Number.isSafeInteger(n)||n<0)n=0; // Giữ giá trị lớn đã lưu để cảnh báo, KHÔNG biến ngầm thành 0.
        return {hours:String(Math.floor(n/60)),minutes:String(n%60).padStart(2,'0')};
    }
    function parseDigits(value) {
        var text=String(value==null?'':value).trim();
        if(!/^\d{1,7}$/.test(text))return null;
        var num=Number(text);
        return Number.isSafeInteger(num)?num:null;
    }
    function readPair(hoursInput,minutesInput) {
        if(!hoursInput||!minutesInput)return null;
        var hours=parseDigits(hoursInput.value),mins=parseDigits(minutesInput.value);
        if(hours===null||mins===null||mins>59||hours>9999)return null;
        var total=hours*60+mins;
        return total<=LIMIT?total:null;
    }
    function bindPart(input,minute) {
        if(!input||input.dataset.kbgPartBound==='1')return;
        input.dataset.kbgPartBound='1';
        input.type='text';input.inputMode='numeric';input.autocomplete='off';input.spellcheck=false;
        input.maxLength=minute?2:4;
        input.setAttribute('pattern',minute?'[0-9]{1,2}':'[0-9]{1,4}');
        input.setAttribute('aria-label',minute?'Số phút, từ 0 đến 59':'Tổng số giờ làm, có thể lớn hơn 100');
        input.addEventListener('beforeinput',function(e){
            if(e.data && /[^0-9]/.test(e.data))e.preventDefault();
        });
        input.addEventListener('input',function(){
            // Chu khong duoc ngam xoa de thanh mot so hop le.
            var parsed=parseDigits(input.value),valid=parsed!==null && (!minute||parsed<=59);
            input.classList.toggle('is-invalid',input.value!==''&&!valid);
            input.setCustomValidity(valid?'':'Vui lòng nhập '+(minute?'phút từ 0 đến 59.':'số giờ bằng chữ số.'));
        });
        input.addEventListener('blur',function(){
            if(minute){var num=parseDigits(input.value);if(num!==null&&num<=59)input.value=String(num).padStart(2,'0');}
        });
    }
    function bindPair(pair) {
        if(!pair)return;
        bindPart(pair.hours,false);bindPart(pair.minutes,true);
    }
    function validatePair(pair) {
        if(!pair)return null;
        bindPair(pair);
        var n=readPair(pair.hours,pair.minutes);
        var good=n!==null;
        pair.hours.classList.toggle('is-invalid',!good);
        pair.minutes.classList.toggle('is-invalid',!good);
        if(!good){
            var m=parseDigits(pair.minutes.value),h=parseDigits(pair.hours.value);
            var target=(h===null||h>9999)?pair.hours:(m===null||m>59)?pair.minutes:pair.hours;
            if(target&&typeof target.focus==='function')target.focus();
        } else {pair.minutes.value=String(n%60).padStart(2,'0');}
        return n;
    }
    function createPair(host,title,totalMinutes) {
        var initial=fromTotal(totalMinutes);
        var field=document.createElement('div');field.className='kbg20-split-time';
        var heading=document.createElement('div');heading.className='kbg20-split-heading';heading.textContent=title;
        field.appendChild(heading);
        if(Number(totalMinutes)>LIMIT){
            var warning=document.createElement('small');
            warning.className='kbg20-hour-legacy-warning';
            warning.textContent='Dữ liệu cũ vượt 9.999 giờ 59 phút. Cần kiểm tra và sửa số giờ đúng; không thể lưu lại số này.';
            field.appendChild(warning);
        }
        var row=document.createElement('div');row.className='kbg20-split-row';field.appendChild(row);
        function part(label,text,cls){
            var wrapper=document.createElement('label');wrapper.className='kbg20-split-part '+cls;
            var input=document.createElement('input');input.className='form-control kbg20-split-input';input.value=text;
            var caption=document.createElement('span');caption.textContent=label;
            wrapper.appendChild(input);wrapper.appendChild(caption);row.appendChild(wrapper);
            return input;
        }
        var hours=part('Giờ',initial.hours,'kbg20-split-hours');
        var minutes=part('Phút',initial.minutes,'kbg20-split-minutes');
        var preview=document.createElement('div');preview.className='kbg20-split-preview';field.appendChild(preview);
        host.appendChild(field);
        var pair={hours:hours,minutes:minutes};bindPair(pair);
        function updatePreview(){
            var n=readPair(hours,minutes);
            if(n===null){
                var typedHours=parseDigits(hours.value);
                preview.textContent=typedHours!==null&&typedHours>9999?'Số giờ đã nhập: '+typedHours.toLocaleString('vi-VN')+' giờ. Giới hạn 9.999 giờ, vui lòng kiểm tra lại.':'Vui lòng nhập Giờ từ 0–9.999 và Phút từ 00–59.';
                preview.classList.add('is-warning');return;}
            var h=Math.floor(n/60),m=n%60;
            preview.textContent='Đang nhập: '+h.toLocaleString('vi-VN')+' giờ '+String(m).padStart(2,'0')+' phút'+(h>1000?' · Số giờ rất lớn, vui lòng kiểm tra!':'');
            preview.classList.toggle('is-warning',h>1000);
        }
        hours.addEventListener('input',updatePreview);minutes.addEventListener('input',updatePreview);
        minutes.addEventListener('blur',updatePreview);updatePreview();
        return pair;
    }
    root.Kbg20HourInput={canonical:canonical,minutes:minutes,bind:bind,validate:validate,
        fromTotal:fromTotal,readPair:readPair,bindPair:bindPair,validatePair:validatePair,createPair:createPair};
})(window);
