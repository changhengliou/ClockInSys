import * as React from 'react';
import NavMenu from './NavMenu';
import '../css/Left_nav.css';
import '../css/login_general.css';
import 'normalize.css';
import { actionCreators } from '../store/index';
import { connect } from 'react-redux';

class Layout extends React.Component {
    constructor(props) {
        super(props);
    }

    render() {
        return (
            <div>
                <NavMenu pathname={this.props.location.pathname}/>
                <div className="mps_content" onClick={() => this.props.onNavBarClose() }>
                    { this.props.body }
                </div>
            </div>
        );
    }
}


const mapDispatchToProps = (dispatch) => {
    return {
        onNavBarClose: () => {
            dispatch(actionCreators.onNavBarClose());
        }
    }
}

const wrapper = connect(null, mapDispatchToProps);

export default wrapper(Layout);
