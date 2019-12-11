import { elements } from './base';

export const render = async () => {
    const markup = `
        <header>
            <nav>
                <div class="main__header__logo">
                    <i class="ion-arrow-graph-up-right"></i>
                    <h2>Desafio Extratos</h2>
                </div>
                <ul>
                    <li><a class="nav_home" href="/">Home</a></li>
                    <li><a class="nav_establishments" href="/">Estabelecimentos</a></li>
                    <li><a class="nav_releases" href="/">Lançamentos</a></li>
                    <li><a class="nav_statements" href="/">Extratos</a></li>
                    <li><a class="nav_stress" href="/">Stress Test</a></li>
                </ul>
            </nav>
            <div class="main__header__content">
                <h1 class="animate bounceIn">Resultados do Desafio Extratos</h1>
                <button>
                    <i class="ion-stats-bars"></i>
                    <p class="nav_establishments">Comece Aqui</p>
                </button>
            </div>
        </header>
    `;
    elements.body.insertAdjacentHTML('beforeend', markup);

    ['nav_establishments', 'nav_releases', 'nav_statements', 'nav_stress'].forEach(item => {
        createScrollNav(item);
    });
}

const createScrollNav = async className => {
    const anchorElements = document.querySelectorAll(`.${className}`);
    
    anchorElements.forEach(anchorElement => {
        anchorElement.addEventListener('click', e => {
            e.preventDefault();
            const worksSection = document.querySelector(`.${className.split('_')[1]}__section`);
            worksSection.scrollIntoView({behavior: 'smooth', block: 'start'});
        });
    });
}