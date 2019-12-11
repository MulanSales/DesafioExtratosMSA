const path = require('path');
const HtmlWebpackPlugin = require('html-webpack-plugin');

module.exports = {
    mode: "development", 
    entry: ['@babel/polyfill', './src/index.js'],
    devtool: 'inline-source-map',
    output: {
        path: path.resolve(__dirname, 'dist'),
        filename: 'js/bundle.js'
    }, 
    devServer: {
        contentBase: path.join(__dirname, "dist")
    },
    plugins: [
        new HtmlWebpackPlugin({
            filename: 'index.html',
            template: './src/index.html'
        })
    ],
    module: {
        rules: [
            {
                test: /\.js$/,
                exclude: /node_modules/,
                use: {
                    loader: 'babel-loader'
                }
            },
            {
                test: /\.css$/,
                use: ['style-loader', 'css-loader']
            },
            {
                test: /\.(woff(2)?|ttf|eot|svg)(\?v=\d+\.\d+\.\d+)?$/,
                use: [
                    {
                        loader: 'file-loader',
                        options: {
                            name: '[name].[ext]',
                            outputPath: 'fonts/'
                        }
                    }
                ]
            },
            {
                test: /\.(gif|png|jpe?g)$/i,
                use: [
                   {
                       loader: 'file-loader', 
                       options: {
                           name: '[name].[ext]',
                           outputPath: 'assets/'
                        }
                   },
                   {
                       loader: 'image-webpack-loader',
                       options: {
                           bypassOnDebug: true,
                           disable: true
                       },
                   },
               ]
            }
        ]
    }
}