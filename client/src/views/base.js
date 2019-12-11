import styles from '../resources/css/styles.css';

/**
 * @var {Object} elements
 */
export const elements = {
    body: document.querySelector('body')
};

/**
 * 
 * @param {number} decimal 
 */
export const formatDecimal = decimal => {
    return "R$ " + decimal.toFixed(2).replace(/\d(?=(\d{3})+\.)/g, '$&,');
};

/**
 * 
 * @param {string} paymentMethod 
 */
export const formatPaymentMethod = paymentMethod => {
    return paymentMethod.replace("e", "é");
};