import { elements, formatDecimal, formatPaymentMethod } from './base';

export const render = async () => {
    const markup = `
        <section class="statements__section">
            <div class="section__title">
                <i class="ion-pie-graph"></i>
                <h1>Extratos</h1>
            </div>
        </section>
    `;
    elements.body.insertAdjacentHTML('beforeend', markup);
}

export const renderTable = async statements => {
    
    let tableRows = '';
    statements.forEach(e => {
        tableRows = tableRows.concat(`
            <tr>
                <td>${e.date}</td>
                <td>${formatPaymentMethod(e.paymentMethod)}</td>
                <td>${e.type}</td>
                <td>${formatDecimal(e.totalAmount)}</td>
            </tr>
        `);
    });
    
    const markup = `
        <table>
            <tr>
                <th>Data</th>
                <th>Forma de Pagamento</th>
                <th>Classificação</th>
                <th>Valor Total</th>
            </tr>
            ${tableRows}
        </table>
    `;

    elements.statementsSection = document.querySelector('.statements__section');
    elements.statementsSection.insertAdjacentHTML('beforeend', markup);
};