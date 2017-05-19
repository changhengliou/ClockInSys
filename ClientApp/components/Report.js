import React, { Component } from 'react';
import { connect } from 'react-redux';
import moment from 'moment';

class Report extends Component {
    render() {
        return (
            <div>
                
            </div>
        );
    }
}

const mapStateToProps = (state) => {
    return {
        data: state.today
    };
}

const mapDispatchToProps = (dispatch) => {
    return {

    }
}

const wrapper = connect(mapStateToProps, mapDispatchToProps);
export default wrapper(Report);
