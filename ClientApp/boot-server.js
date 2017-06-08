import * as React from 'react';
import { Provider } from 'react-redux';
import { renderToString } from 'react-dom/server';
import { match, RouterContext } from 'react-router';
import createMemoryHistory from 'history/lib/createMemoryHistory';
import { createServerRenderer, RenderResult } from 'aspnet-prerendering';
import routes, { ProtectedUrlMapping } from './routes';
import configureStore from './configureStore';

export default createServerRenderer(params => {
    return new Promise((resolve, reject) => {
        // Match the incoming request against the list of client-side routes
        const store = configureStore({ __info__: params.data });
        match({ routes, location: params.location }, (error, redirectLocation, renderProps) => {
            if (error) {
                throw error;
            }

            // If there's a redirection, just send this information back to the host application
            if (redirectLocation) {
                resolve({ redirectUrl: redirectLocation.pathname });
                return;
            }

            // If it didn't match any route, renderProps will be undefined
            if (!renderProps) {
                throw new Error(`The location '${ params.url }' doesn't match any route configured in react-router.`);
            }

            if (params.data.roles[0] === 'default') {
                if (ProtectedUrlMapping[params.url]) {
                    throw new Error(`Access denied.`);
                }
            }
            // Build an instance of the application
            const app = (
                <Provider store={ store }>
                    <RouterContext {...renderProps} />
                </Provider>
            );

            // Perform an initial render that will cause any async tasks (e.g., data access) to begin
            renderToString(app);

            // Once the tasks are done, we can perform the final render
            // We also send the redux store state, so the client can continue execution where the server left off
            params.domainTasks.then(() => {
                resolve({
                    html: renderToString(app),
                    globals: { initialReduxState: store.getState() }
                });
            }, reject); // Also propagate any errors back into the host application
        });
    });
});
