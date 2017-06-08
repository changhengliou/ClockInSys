import React, {Component} from 'react';
import {connect} from 'react-redux';
import moment from 'moment';
import OffOption from './DayOff.Options';

export const style = {
    title: {
        fontWeight: '900', textAlign: 'center', marginBottom: '8px'
    },
    label: {
        width: '110px',
        display: 'inline-block',
    },
    input: {
        display: 'inline-block',
        textAlign: 'center',
        width: '50%',
        height: '26px'
    },
    textarea: { width: '50%' },
    select: { width: '50%', height: '26px' },
    wrapper: { marginBottom: '8px' },
    disabled: { backgroundColor: '#ccc', borderColor: '#bbb' }
};

class DialogContent extends Component {
    shouldComponentUpdate(nextProps) {
        var props = this.props.data;
        var next = nextProps.data;
        return !(JSON.stringify(props) === JSON.stringify(next));
    }
    render() {
        var props = this.props.data;
        var disabledNormal = !props.offType && !props.checkInTime && !props.checkOutTime ? true : false;
        var disabledOff = !props.offType || 
                          (props.offType && props.offTimeStart && props.offTimeEnd) ? 
                          false : true;
        var disabledConfirmBtn = disabledNormal || disabledOff;
        if (props.checkInTime === '' && this.refs.checkInTime) {
            this.refs.checkInTime.value = '';
        }
        if (props.checkOutTime === ''  && this.refs.checkOutTime) {
            this.refs.checkOutTime.value = '';
        }
        if (props.offTimeStart === ''  && this.refs.offTimeStart) {
            this.refs.offTimeStart.value = '';
        }
        if (props.offTimeEnd === ''  && this.refs.offTimeEnd) {
            this.refs.offTimeEnd.value = '';
        }
        var showCheckIn = this.props.showCheckIn ? (
            <div>
                <div style={style.wrapper}>
                    <span style={style.label}>上班時間:</span>
                    <input style={style.input}
                           type='time'
                           name='checkInTime' 
                           ref='checkInTime'
                           value={ props.checkInTime ? props.checkInTime.slice(0, 8) : null }
                           onChange={ (e) => this.props.onInputChange(e.target.value, e.target.name) }/>
                </div>
                <div style={style.wrapper}>
                    <span style={style.label}>下班時間:</span>
                    <input style={style.input}
                           type='time' 
                           name='checkOutTime'
                           ref='checkOutTime'
                           value={ props.checkOutTime ? props.checkOutTime.slice(0, 8) : null }
                           onChange={ (e) => this.props.onInputChange(e.target.value, e.target.name) }/>
                </div>
            </div>
        ) : null;
        var showGeo = this.props.showGeo ? (
            <div>
                <div style={style.wrapper}>
                    <span style={style.label}>上班打卡座標:</span>
                    <input style={style.input}
                           type='text' 
                           name='geoLocation1'
                           value={ props.geoLocation1 }
                           onChange={ (e) => this.props.onInputChange(e.target.value, e.target.name) }/>
                </div>
                <div style={style.wrapper}>
                    <span style={style.label}>下班打卡座標:</span>
                    <input style={style.input}
                           type='text' 
                           name='geoLocation2'
                           value={ props.geoLocation2 }
                           onChange={ (e) => this.props.onInputChange(e.target.value, e.target.name) }/>
                </div>
            </div>
        ) : null;
        var showOff = this.props.showOff ? (
            <div>
                <div style={style.wrapper}>
                    <span style={style.label}>請假類別:</span>
                    <OffOption value={ props.offType } 
                               style={style.select}
                               showNoneOption
                               sickLeaves={ this.props.s }
                               annualLeaves={ this.props.a }
                               familyCareLeaves={ this.props.f }
                               onChange={ (e) => this.props.onInputChange(e.target.value, e.target.name) }/>
                </div>
                <div style={style.wrapper}>
                    <span style={style.label}>請假時間(起):</span>
                    <input style={ !props.offType ? { ...style.input, ...style.disabled } : style.input }
                           type='time' 
                           value={ props.offTimeStart }
                           name='offTimeStart'
                           ref='offTimeStart'
                           disabled={ !props.offType ? true : false }
                           onChange={ (e) => this.props.onInputChange(e.target.value, e.target.name) }/>
                </div>
                <div style={style.wrapper}>
                    <span style={style.label}>請假時間(迄):</span>
                    <input style={ !props.offType ? {...style.input, ...style.disabled } : style.input }
                           type='time' 
                           value={ props.offTimeEnd }
                           name='offTimeEnd'
                           ref='offTimeEnd'
                           disabled={ !props.offType ? true : false }
                           onChange={ (e) => this.props.onInputChange(e.target.value, e.target.name) }/>
                </div>
                <div style={style.wrapper}>
                    <span style={style.label}>請假原因:</span>
                    <textarea style={ !props.offType ? {...style.textarea, ...style.disabled } : style.textarea }
                              value={ props.offReason }
                              name='offReason'
                              disabled={ !props.offType ? true : false }
                              onChange={ (e) => this.props.onInputChange(e.target.value, e.target.name) }/>
                </div>
            </div>
        ) : null;
        var showStatus = this.props.showStatus ? (
            <div>
                <span style={style.label}>狀態:</span>
                <select style={ !props.offType ? {...style.select, ...style.disabled } :style.select} 
                        value={ props.statusOfApproval }
                        name='statusOfApproval'
                        disabled={ !props.offType ? true : false }
                        onChange={ (e) => this.props.onInputChange(e.target.value, e.target.name) }>
                    <option value='審核中'>審核中</option>
                    <option value='已核准'>已核准</option>
                    <option value='遭駁回'>遭駁回</option>
                </select>
            </div>
        ) : null;
        var showBtn = this.props.showBtn ? (
            <div>
                <button className='btn_date btn_date_group btn_default'
                        disabled={ disabledConfirmBtn }
                        style={ disabledConfirmBtn ? style.disabled : null }
                        onClick={ () => this.props.onSubmitReport() }>確認</button>
                <button className='btn_date btn_date_group btn_danger'
                        style={ props.recordId ? null : style.disabled }
                        disabled={ props.recordId ? false : true }
                        onClick={ () => this.props.onDeleteReport() }>刪除紀錄</button>
                <button className='btn_date btn_date_group btn_info'
                        onClick={ () => this.props.onDialogClose() }>取消</button>
            </div>
        ) : null;
        return (
            <div className='selectReport'>
                <div style={ style.title }>{ props.userName }</div>
                <div style={ style.title }>
                    { new moment(props.checkedDate, 'YYYY-MM-DD').format('YYYY年MM月DD日') }
                </div>
                { showCheckIn }
                { showGeo }
                { showOff }
                { showStatus }
                { showBtn }
            </div>
        );
    }
}

const mapStateToProps = (state) => {
    return {
        data: state.report.model
    };
}

const mapDispatchToProps = (dispatch) => {
    return {
        
    }
}

const wrapper = connect(mapStateToProps, mapDispatchToProps);
export default wrapper(DialogContent);
