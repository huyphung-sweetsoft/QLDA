class TwoFactorAuthenticator {
    elementOTPs;
    constructor(idHdfOTP, idFormTwoFA) {
        this.idHdfOTP = idHdfOTP;
        this.idFormTwoFA = idFormTwoFA;
    }
    init = () => {
        this.elementOTPs = document.querySelectorAll(`#${this.idFormTwoFA} .otp-input input`);
        this.elementOTPs.forEach((input, index) => {
            input.addEventListener('input', (e) => {
                if (e.target.value.length > 1) {
                    e.target.value = e.target.value.slice(0, 1);
                }
                if (e.target.value.length === 1) {
                    if (index < this.elementOTPs.length - 1) {
                        this.elementOTPs[index + 1].focus();
                    }
                }
            });

            input.addEventListener('keydown', (e) => {
                if (e.key === 'ArrowLeft' || (e.key === 'Backspace' && !e.target.value)) {
                    if (index > 0) {
                        const prevElement = this.elementOTPs[index - 1];
                        prevElement.focus();
                        prevElement.select();
                    }
                }
                else if (e.key === 'ArrowRight') {
                    if (index < this.elementOTPs.length - 1) {
                        const nextElement = this.elementOTPs[index + 1];
                        nextElement.focus();
                        nextElement.select();
                    }
                }
                if (e.key === 'e') {
                    e.preventDefault();
                }
            });
        });
    }
    verifyOTP = () => {
        const isValid = CMSMasterJs.ValidElement(`#${this.idFormTwoFA}.form-two-factor-authentication`);
        if (!isValid) {
            toastr.error('Please enter a 6 digit OTP!');
            return false;
        }

        const otp = Array.from(this.elementOTPs).map(input => input.value).join('');
        if (otp.length === 6) {
            $(`#${this.idHdfOTP}`).val(otp);
            return true;
        } else {
            toastr.error('Please enter a 6 digit OTP!');
            return false;
        }
    }
}