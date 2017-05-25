import React, { Component } from 'react';
import { connect } from 'react-redux';
import moment from 'moment';
import Select from 'react-select';
import 'react-select/dist/react-select.css';
import { getOptions } from './ManageAccount';
import { actionCreators } from '../store/reportStore';

class Report extends Component {
    constructor(props) {
        super(props);
        var today = new moment();
        this.searchOptions = [{
            label: '全部', value: '0'
        }, {
            label: '出勤', value: '1'
        }, {
            label: '請假', value: '2'
        }];
        this.dateOptions = [{
            label: `今日 (${today.add(1, 'days').format('YYYY-MM-DD')})`, value: '0'
        }, {
            label: `7日內 (${today.add(-7, 'days').format('YYYY-MM-DD')}~)`, value: '1'
        }, {
            label: `14日內 (${today.add(-14, 'days').format('YYYY-MM-DD')}~)`, value: '2'
        }, {
            label: `30日內 (${today.add(-30, 'days').format('YYYY-MM-DD')}~)`, value: '3'
        }, {
            label: `90日內 (${today.add(-90, 'days').format('YYYY-MM-DD')}~)`, value: '4'
        }, {
            label: '其他', value: '-1'
        }];
    }

    render() {
        var props = this.props.data;
        return (
            <div className='selectReport'>
                <div>
                    <label style={{width:'20%', marginRight: '3%',fontSize: '20px'}}>姓名:</label>
                    <div style={{width: '45%', marginRight: '3%', display: 'inline-block', height:'25px'}}>
                        <Select.Async disabled={ props.all }
                                      value={ props.name }
                                      onChange={ this.props.onNameChanged }
                                      clearable={false}
                                      ignoreCase={true}
                                      loadOptions={ getOptions }></Select.Async>
                    </div>
                    <input style={{width: '3%'}}type="checkbox" checked={props.all} onChange={() => this.props.onAllChanged()}/>
                    <label style={{width: '26%'}}>全體員工</label>
                </div>
                <div style={{marginTop: '16px'}}>
                    <label style={{width:'20%', marginRight: '3%',fontSize: '20px'}}>種類:</label>
                    <div style={{width: '45%', marginRight: '32%', display: 'inline-block', height:'25px'}}>
                        <Select searchable={ false } 
                                clearable={ false }
                                value={ props.options }
                                onChange={ (s) => this.props.onOptionsChanges(s) }
                                options={ this.searchOptions }></Select>
                    </div>
                </div>
                <div style={{marginTop: '16px'}}>
                    <label style={{width:'20%', marginRight: '3%',fontSize: '20px'}}>時間:</label>
                    <div style={{width: '45%', marginRight: '32%', display: 'inline-block', height:'25px'}}>
                        <Select searchable={ false } 
                                clearable={ false }
                                value={ props.dateOptions }
                                onChange={ (s) => this.props.onDateOptionsChanges(s) }
                                options={ this.dateOptions }></Select>
                    </div>
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
        }
    }
}

const wrapper = connect(mapStateToProps, mapDispatchToProps);
export default wrapper(Report);
