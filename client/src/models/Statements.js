import Model from "./Model";

export default class Statements extends Model {
    constructor() { super(); }

    async getStatements() {
        const establishments = await fetch(
            `${this.url}api/statements`, {
                method: 'GET',
                headers: {
                    'Content-Type': 'application/json'
                }
            }
        );

        const fetchedResource = await establishments.json();
        
        return fetchedResource;
    }
}