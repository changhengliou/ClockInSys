import React, { Component } from 'react';
import { connect } from 'react-redux';
import moment from 'moment';
import Select from 'react-select';
import 'react-select/dist/react-select.css';
import { getOptions } from './ManageAccount';
import { actionCreators, getDateOptions } from '../store/reportStore';
import DatePicker from 'react-datepicker';
import 'react-datepicker/dist/react-datepicker.min.css';
import { DateInput } from './DayOff';

class Form extends Component {
    constructor(props) {
        super(props);
        this.searchOptions = [{
            label: '全部', value: '0'
        }, {
            label: '出勤', value: '1'
        }, {
            label: '請假', value: '2'
        }, {
            label: '加班', value: '3'
        }, {
            label: '其他', value: '4'
        }];
        this.dateOptions = [{
            label: `今日 (${getDateOptions('0')})`, value: '0'
        }, {
            label: `7日內 (${getDateOptions('1')}~)`, value: '1'
        }, {
            label: `14日內 (${getDateOptions('2')}~)`, value: '2'
        }, {
            label: `30日內 (${getDateOptions('3')}~)`, value: '3'
        }, {
            label: `90日內 (${getDateOptions('4')}~)`, value: '4'
        }, {
            label: '其他', value: '-1'
        }];
    }

    render() {
        var props = this.props.data;
        var dateOpt = props.dateOptions === '-1' ? (
            <div>
                <div style={{marginTop: '16px'}}>
                    <label style={{width:'50px', marginRight: '3%',fontSize: '20px',display: 'inline-block'}}>起:</label>
                    <div style={{width: '45%', marginRight: '88px', display: 'inline-block', height:'25px'}}>
                        <DatePicker name="from" selectsStart dropdownMode="select"
                            showMonthDropdown showYearDropdown
                            customInput={<DateInput/>} 
                            onChange={ (e) => { this.props.onFromDateChange(e) } }
                            selected={ props.fromDate } 
                            startDate={ props.fromDate }
                            endDate={ props.toDate }/>
                    </div>
                </div>
                <div style={{marginTop: '16px'}}>
                    <label style={{width:'50px', marginRight: '3%',fontSize: '20px',display: 'inline-block'}}>訖:</label>
                    <div style={{width: '45%', marginRight: '88px', display: 'inline-block', height:'25px'}}>
                        <DatePicker name="to" selectsEnd dropdownMode="select"
                            showMonthDropdown showYearDropdown
                            customInput={<DateInput/>} 
                            onChange={ (e) => { this.props.onToDateChange(e) } }
                            selected={ props.toDate } 
                            startDate={ props.fromDate }
                            endDate={ props.toDate }/>
                    </div>
                </div>
            </div>
        ) : null;
        return (
            <div className='selectReport'>
                <div>
                    <label style={{width:'50px', marginRight: '3%',fontSize: '20px',display: 'inline-block'}}>姓名:</label>
                    <div style={{width: '45%', marginRight: '10px', display: 'inline-block', height:'25px'}}>
                        <Select.Async disabled={ props.all }
                                      value={ props.id }
                                      onChange={ this.props.onNameChanged }
                                      clearable={false}
                                      ignoreCase={true}
                                      loadOptions={ getOptions }></Select.Async>
                    </div>
                    <input style={{width: '12px'}}type="checkbox" checked={props.all} onChange={() => this.props.onAllChanged()}/>
                    <label style={{width: '88px'}}>全體員工</label>
                </div>
                <div style={{marginTop: '16px'}}>
                    <label style={{width:'50px', marginRight: '3%',fontSize: '20px',display: 'inline-block'}}>種類:</label>
                    <div style={{width: '45%', marginRight: '88px', display: 'inline-block', height:'25px'}}>
                        <Select searchable={ false } 
                                clearable={ false }
                                value={ props.options }
                                onChange={ (s) => this.props.onOptionsChanges(s) }
                                options={ this.searchOptions }></Select>
                    </div>
                </div>
                <div style={{marginTop: '16px'}}>
                    <label style={{width:'50px', marginRight: '3%',fontSize: '20px',display: 'inline-block'}}>時間:</label>
                    <div style={{width: '45%', marginRight: '88px', display: 'inline-block', height:'25px'}}>
                        <Select searchable={ false } 
                                clearable={ false }
                                value={ props.dateOptions }
                                onChange={ (s) => this.props.onDateOptionsChanges(s) }
                                options={ this.dateOptions }></Select>
                    </div>
                </div>
                { dateOpt }
                <div style={{width: '30%', margin: '0 auto', marginTop: '16px'}}>
                    <button className='btn_date btn_default' onClick={ () => this.props.query() }>查詢</button>
                </div>
            </div>
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
        onNameChanged: (s) => {
            dispatch(actionCreators.onNameChanged(s.value));
        },
        onAllChanged: () => {
            dispatch(actionCreators.onAllChanged());
        },
        onOptionsChanges: (s) => {
            dispatch(actionCreators.onOptionsChanges(s.value));
        },
        onDateOptionsChanges: (s) => {
            dispatch(actionCreators.onDateOptionsChanges(s.value));
        },
        onFromDateChange: (s) => {
            dispatch(actionCreators.onFromDateChange(s));
        },
        onToDateChange: (s) => {
            dispatch(actionCreators.onToDateChange(s));
        },
        query: () => {
            dispatch(actionCreators.query());
        }
    }
}

const wrapper = connect(mapStateToProps, mapDispatchToProps);
export default wrapper(Form);
