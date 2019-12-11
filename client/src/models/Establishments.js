import Model from "./Model";

export default class Establishments extends Model {
    constructor() { super(); }

    async getEstablishments() {
        const establishments = await fetch(
            `${this.url}api/establishments`, {
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