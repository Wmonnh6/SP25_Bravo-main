/**
 * This class is the model for requests to create invitations.
 * This is written in a style similar to a Java or other OOP class because
 * I think it reads easier for another developer even if they lack Angular
 * knowledge.
 */
export class InvitationRequest {
    private _email: string;
    private _isAdmin: boolean;

    constructor(email: string, admin: boolean) {
        this._email = email;
        this._isAdmin = admin;
    }

    get email() {
        return this._email;
    }

    set email(value: string) {
        this._email = value;
    }

    get isAdmin() {
        return this._isAdmin;
    }

    set isAdmin(value: boolean) {
        this._isAdmin = value;
    }
}