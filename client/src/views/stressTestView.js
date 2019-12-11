import { elements } from './base';

export const render = async () => {
    const markup = `
        <section class="stress__section">
            <div class="section__title">
                <i class="ion-pie-graph"></i>
                <h1>Teste de Stress</h1>
            </div>
        </section>
    `;

    elements.body.insertAdjacentHTML('beforeend', markup);
};

export const renderContainer = async () => {

    return fetch('/artillery', {
        method: 'GET',
        headers: {
            'Content-Type' : 'application/json'
        }
    })
    .then(fetchedData => {
        return fetchedData.json();
    })
    .then(data => {
        const aggregate = data.aggregate;

        const tableRows = `
            <table class="stress__table">
                <tr>
                    <th>Data</th>
                    <th>Cenários Criados</th>
                    <th>Cenários Completados</th>
                    <th>Requisições Completadas</th>
                    <th>Latência Mínima</th>
                    <th>Latência Mediana</th>
                    <th>Latência Máxima</th>
                    <th>Cenários (Status Code : 200)</th>
                    <th>Porcentagem de Sucesso</th>
                </tr>
                <tr>
                    <th>${aggregate.timestamp.split("T")[0]} ${aggregate.timestamp.split("T")[1].split(".")[0]}</th>
                    <th>${aggregate.scenariosCreated}</th>
                    <th>${aggregate.scenariosCompleted}</th>
                    <th>${aggregate.requestsCompleted}</th>
                    <th>${Math.round(aggregate.latency.min)} ms</th>
                    <th>${Math.round(aggregate.latency.median)} ms</th>
                    <th>${Math.round(aggregate.latency.max)} ms</th>
                    <th>${aggregate.codes["200"]}</th>
                    <th>${Math.floor(100*(aggregate.scenariosCompleted/aggregate.scenariosCreated))}%</th>
                </tr>
            </table>
        `;

        const container = `
            <div class="stress__container">
                <i class="ion-speedometer"></i>
                <div class="stress__buttons">
                    <button>
                        <i class="ion-android-arrow-dropright-circle"></i>
                        <p>Iniciar Teste</p>
                    </button>
                    <blockquote>O teste dura 60 segundos"</blockquote>
                </div>
                <div class="stress__results">
                    <h3>Resumo dos Resultados</h3>
                    ${tableRows}
                    <a href="/artillery">Resultados Completos em JSON</a>
                </div>
                <h2>O teste de stress é realizado utilizando o pacote <emph style="color:#2ecc70c4">Artillery</emph> para Node.js</h2>
            </div>
        `;

        elements.stressSection = document.querySelector('.stress__section');
        elements.stressSection.insertAdjacentHTML('beforeend', container);

        document.querySelector('.stress__buttons button').addEventListener('click', startTest);
    });
};

const startTest = async () => {
    const markup = `
        <div class="modal__container">
            <p class="modal__testing">Realizando Testes</p>
            <div class="lds-ring">
                <div></div><div></div><div></div><div></div>
            </div>      
            <p class="modal__waiting">Aguarde...</p>  
        </div>
    `;


    const stressSection = document.querySelector('.stress__section');
    stressSection.insertAdjacentHTML('beforeend', markup);

    const stress__buttons = document.querySelector('.stress__buttons');
    stress__buttons.style.display = "none";

    await fetch('/artillery', {
        method: 'POST',
        headers: {
            'Content-Type' : 'application/json'
        }
    });

    window.setTimeout(() => {
        const modalContainer = document.querySelector('.modal__container');
        modalContainer.parentNode.removeChild(modalContainer);

        const stressContainer = document.querySelector('.stress__container');
        stressContainer.parentNode.removeChild(stressContainer);

        renderContainer();
        setTimeout(() => {
            const newStressContainer = document.querySelector('.stress__container');
            newStressContainer.scrollIntoView({behavior: 'smooth', block: 'start'});
        }, 2000);

    }, 60000);
};
