import { elements } from './base';

export const render = async () => {
    const markup = `
        <section class="establishments__section">
            <div class="section__title">
                <i class="ion-pie-graph"></i>
                <h1>Estabelecimentos</h1>
            </div>
        </section>
    `;
    elements.body.insertAdjacentHTML('beforeend', markup);
}

export const renderTable = async establishments => {
    
    let tableRows = '';
    establishments.forEach(e => {
        tableRows = tableRows.concat(`
            <tr>
                <td>${e.name}</td>
                <td>${e.type}</td>
            </tr>
        `);
    });
    
    const markup = `
        <table>
            <tr>
                <th>Estabelecimento</th>
                <th>Classificação</th>
            </tr>
            ${tableRows}
        </table>
    `;

    elements.establishmentsSection = document.querySelector('.establishments__section');
    elements.establishmentsSection.insertAdjacentHTML('beforeend', markup);
};