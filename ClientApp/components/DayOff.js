import React, { Component } from 'react';
import DatePicker from 'react-datepicker';
import { connect } from 'react-redux';
import moment from 'moment';
import { actionCreators } from '../store/dayoffStore';
import 'react-datepicker/dist/react-datepicker.min.css';
import '../css/manageAccount.css';
import '../css/dayoff.css';

export class DateInput extends Component {
    render() {
        var style = {};
        if(this.props.disabled)
            style = {backgroundColor: '#ccc'};
        return (
            <button className='btn_date' onClick={this.props.onClick} style={style}>
                {this.props.value}
            </button>
        );
    }
}

DateInput.propTypes = {
    value: React.PropTypes.string,
    onClick: React.PropTypes.func
};

const style = {
    label: { width: '160px' },
    textarea: { height: '60px' },
    select : { textAlign: 'center', width: '43%' },
    type: { textAlign: 'center' },
    disabled : { backgroundColor: '#ccc', borderColor: '#bbb' }
};

class DayOff extends Component {
    constructor(props) {
        super(props);
        this.renderHourOptions = this.renderHourOptions.bind(this);
    }

    renderHourOptions(isToday = false) {
        var time = new Date();
        var hours = time.getHours() + 1;
        var hourOption = isToday ? [] : 
                                   [9, 10, 11, 12, 13, 14, 15, 16, 17, 18];
        var options = [];
        if (isToday) {
            for(var i = hours; i <= 18; i++) {
                hourOption.push(i);
            } 
        }
        hourOption.map((obj) => {
            options.push(<option value={obj}>{obj}:00</option>);
        });
        return options;
    }
    
    render() {
        var isToday = false,
            diffDate = false,
            props = this.props.data,
            timeDiff = props.toTime - props.fromTime,
            fromDate = new moment(props.fromDate, 'YYYY-MM-DD'),
            toDate = new moment(props.toDate, 'YYYY-MM-DD'),
            disabledDate = props.disabledContent !== 'none' ? true : false,
            disabledAll = props.disabledContent === 'all' ? true : false,
            disabledPartial = props.disabledContent === 'partial';

        if(props.fromDate !== props.toDate) {
            diffDate = true;
        } else {
            if (new moment(props.fromDate, 'YYYY-MM-DD').isSame(new moment(), 'days'))
                isToday = true;
        }
        return (
            <div>
                <div style={{margin: '0px auto', width: '60%', textAlign: 'left'}}>
                    <div className="dialog_row">
                        <label className="mps_content_label" style={style.label}>(起)時間:</label>
                        <DatePicker id="from" selectsStart dropdownMode="select"
                            showMonthDropdown disabled
                            minDate={ fromDate }
                            customInput={<DateInput disabled/>} 
                            selected={ fromDate } 
                            startDate={ fromDate }
                            endDate={ toDate }/>
                    </div>
                    <div className="dialog_row">
                        <label className="mps_content_label" style={style.label}>(迄)時間:</label>
                        <DatePicker id="to" selectsEnd dropdownMode="select"
                        showMonthDropdown disabled={ disabledDate }
                        minDate={ fromDate }
                        customInput={<DateInput disabledDate/>} 
                        onChange={ (e) => { this.props.onDateChange(e) } }
                        selected={ toDate } 
                        startDate={ fromDate }
                        endDate={ toDate }/>
                    </div>
                    <div className="dialog_row">
                        <label className="mps_content_label" style={style.label}>種類:</label>
                        <select id="offType" 
                                className="dayOffInput"
                                disabled={ disabledAll } 
                                style={ disabledAll ? { ...style.type, ... style.disabled} : style.type }
                                value={ props.offType }
                                onChange={ (e) => this.props.onOffTypeChange(e.target.value) }>
                            <option value="事假">事假</option>
                            <option value="病假">病假</option>
                            <option value="喪假">喪假</option>
                            <option value="公假">公假</option>
                            <option value="特休">特休</option>
                            <option value="家庭照顧假">家庭照顧假</option>
                            <option value="補休">補休</option>
                            <option value="婚假">婚假</option>
                            <option value="陪產假">陪產假</option>
                            <option value="其他">其他</option>
                        </select>
                    </div>
                    <div className="dialog_row">
                        <label className="mps_content_label" style={style.label}>期間:
                            <font style={{color: 'red', marginLeft: '10px'}}>{ timeDiff ? `${timeDiff}小時` : '無效的時間'}</font>
                        </label>
                        <div>
                            <select name="fromTime" className="dayOffInput" disabled={diffDate}
                                    value={ props.fromTime }
                                    style={ disabledAll || diffDate ? { ...style.select, ...style.disabled } : style.select } 
                                    disabled={ disabledAll || diffDate }
                                    onChange={ (e) => this.props.onTimeChange(e.target.name, e.target.value) }>
                                {
                                    diffDate ? <option value='9'>9:00</option> : 
                                               this.renderHourOptions(isToday).map((i) => {
                                                   return i;
                                               })
                                }
                            </select>
                            <label style={{fontSize: '20px', color: '#4a4a4a', width: '14%', textAlign: 'center'}}>至</label>
                            <select name="toTime" className="dayOffInput" 
                                    value={ props.toTime }
                                    style={ diffDate || disabledAll ? { ...style.select, ...style.disabled } : style.select } 
                                    disabled={ disabledAll || diffDate }
                                    onChange={ (e) => this.props.onTimeChange(e.target.name, e.target.value) }>
                                {
                                    diffDate ? <option value='18'>18:00</option> : 
                                               this.renderHourOptions(isToday).map((i) => {
                                                   return i;
                                               })
                                }
                            </select>
                        </div>
                    </div>
                    <div className="dialog_row">
                        <label className="mps_content_label" style={style.label}>請假事由:</label>
                        <textarea id="offReason" 
                                  maxLength="100"
                                  disabled={ disabledAll }
                                  value={ props.offReason }
                                  onChange={ (e) => this.props.onOffReasonChange(e.target.value) }
                                  style={ disabledAll ? { ...style.textarea, ...style.disabled } : style.textarea }
                                  className="dayOffInput"></textarea>
                    </div>
                    <div className="dialog_row">
                        <button className="btn-2-group btn_date btn_info" disabled={ !timeDiff || disabledAll ? true : false }
                                style={ !timeDiff || disabledAll ? style.disabled : null }
                                onClick={ () => { this.props.onDialogConfirm() } }>{ disabledDate ? '修改' : '確認' }</button>
                        <button className={ disabledPartial ? 'btn_delete btn-2-group btn_date' : 'btn_trans_blue btn-2-group btn_date'}
                                onClick={ () => {
                                    if(disabledPartial)
                                        this.props.onCancelLeaves(props.fromDate)
                                    else
                                        this.props.onDialogClose()
                                } }>
                                { disabledPartial ? '取消請假' : '關閉' }
                        </button>
                    </div>
                </div>
            </div>
        );
    }
}

const mapStateToProps = (state) => {
    return {
        data: state.dayoff
    };
}

const mapDispatchToProps = (dispatch) => {
    return {
        onDateChange: (date) => {
            dispatch(actionCreators.onDateChange(date.format('YYYY-MM-DD')));
        },
        onTimeChange: (name, value) => {
            dispatch(actionCreators.onTimeChange(name, value));
        },
        onOffTypeChange: (value) => {
            dispatch(actionCreators.onOffTypeChange(value));
        },
        onOffReasonChange: (value) => {
            dispatch(actionCreators.onOffReasonChange(value));
        },
        onDialogOpen: () => {
            dispatch(actionCreators.onDialogOpen());
        },
        onDialogClose: () => {
            dispatch(actionCreators.onDialogClose());
        },
        onDialogConfirm: () => {
            dispatch(actionCreators.onDialogConfirm());
        },
        onCancelLeaves: (date) => {
            dispatch(actionCreators.onCancelLeaves(date));
        }
    }
}
const wrapper = connect(mapStateToProps, mapDispatchToProps);
export default wrapper(DayOff);
