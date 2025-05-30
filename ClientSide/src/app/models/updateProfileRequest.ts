export class UpdateProfileRequest {
    private _email: string;
    private _firstName: string;
    private _lastName: string;
    private _newPass: string;
    private _confirmPass: string;

    constructor(email: string, firstName: string, lastName: string, password: string, confPass: string) {
        this._email = email;
        this._firstName = firstName;
        this._lastName = lastName;
        this._newPass = password;
        this._confirmPass = confPass;
    }

    get email() {
        return this._email;
    }

    set email(value: string) {
        this._email = value;
    }

    get firstName() {
        return this._firstName;
    }

    set firstName(value: string) {
        this._firstName = value;
    }

    get lastName() {
        return this._lastName;
    }

    set lastName(value: string) {
        this._lastName = value;
    }

    get newPass() {
        return this._newPass;
    }

    set newPass(value: string) {
        this._newPass = value;
    }

    get confirmPass() {
        return this._confirmPass;
    }

    set confirmPass(value: string) {
        this._confirmPass = value;
    }
}