import Model from "./Model";

export default class Releases extends Model {
    constructor() { super(); }

    async getReleases() {
        const establishments = await fetch(
            `${this.url}api/releases`, {
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