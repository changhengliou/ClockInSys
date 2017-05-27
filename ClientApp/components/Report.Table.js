import React, { Component } from 'react';
import { connect } from 'react-redux';
import moment from 'moment';
import Select from 'react-select';
import 'react-select/dist/react-select.css';
import { getOptions } from './ManageAccount';
import { actionCreators } from '../store/reportStore';
import DatePicker from 'react-datepicker';
import 'react-datepicker/dist/react-datepicker.min.css';
import { DateInput } from './DayOff';


class Table extends Component {
    render() {
        var props = this.props.data;
        return (
            <div>table</div>
        );
    }
}
const mapStateToProps = (state) => {
    return {
        data: state.report
    };
}

const mapDispatchToProps = (dispatch) => {
    return {
        
    }
}

const wrapper = connect(mapStateToProps, mapDispatchToProps);
export default wrapper(Table);

