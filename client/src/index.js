import * as homePageView from './views/homePageView';
import * as establishmentView from './views/establishmentsView';
import * as releasesView from './views/releasesView';
import * as statementsView from './views/statementsView';
import * as stressTestView from './views/stressTestView';

import Establishments from './models/Establishments';
import Releases from './models/Releases';
import Statements from './models/Statements';


/** Global state of app
 * @var {object} state 
 */
const state = {};

window.addEventListener('load', () => homePageController());

/** 
 * Home Page Controller
 */
const homePageController = async () => {
    // 1: Render home page view
    homePageView.render();

    // 2: Render establishment view
    establishmentView.render();

    // 3: Get Establishment Data
    const establishments = await new Establishments().getEstablishments();
    establishmentView.renderTable(establishments);

    // 4: Render releases section
    releasesView.render();

    // 5: Get Releases Data
    const releases = await new Releases().getReleases();
    releasesView.renderTable(releases);

    // 6: Render statements section
    statementsView.render();

    // 7: Get Statements Data
    const statements = await new Statements().getStatements();
    statementsView.renderTable(statements);

    return await stressTestController();
};

/** 
 * Stress Test Controller
 */
const stressTestController = async () => {

    // 1: Render stress test section
    stressTestView.render();
    stressTestView.renderContainer();

};

