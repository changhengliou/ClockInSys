import * as React from 'react';
import NavMenu from './NavMenu';
import '../css/Left_nav.css';
import '../css/login_general.css';
import 'normalize.css';

export class Layout extends React.Component {
    constructor(props) {
        super(props);
    }

    render() {
        return (
            <div>
                <NavMenu pathname={this.props.location.pathname}/>
                <div className="mps_content" onClick={this.closeNav}>
                    { this.props.body }
                </div>
            </div>
        );
    }
}
