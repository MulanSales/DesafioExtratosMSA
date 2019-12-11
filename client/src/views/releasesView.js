import { elements, formatDecimal, formatPaymentMethod } from './base';

export const render = async () => {
    const markup = `
        <section class="releases__section">
            <div class="section__title">
                <i class="ion-pie-graph"></i>
                <h1>Lançamentos</h1>
            </div>
        </section>
    `;
    elements.body.insertAdjacentHTML('beforeend', markup);
}

export const renderTable = async releases => {
    
    let tableRows = '';
    releases.forEach(e => {
        tableRows = tableRows.concat(`
            <tr>
                <td>${e.date}</td>
                <td>${formatPaymentMethod(e.paymentMethod)}</td>
                <td>${e.establishmentName}</td>
                <td>${formatDecimal(e.amount)}</td>
            </tr>
        `);
    });
    
    const markup = `
        <table>
            <tr>
                <th>Data</th>
                <th>Forma de Pagamento</th>
                <th>Estabelecimento</th>
                <th>Valor</th>
            </tr>
            ${tableRows}
        </table>
    `;

    elements.releasesSection= document.querySelector('.releases__section');
    elements.releasesSection.insertAdjacentHTML('beforeend', markup);
};