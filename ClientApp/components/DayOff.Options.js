import React, {Component} from 'react';
import {connect} from 'react-redux';
import moment from 'moment';

const style = {
    type: { textAlign: 'center' },
    disabled : { backgroundColor: '#ccc', borderColor: '#bbb' }
};
class OffOption extends Component {
    render() {
        var sickLeavesOnly = this.props.sickLeavesOnly;
        var isDisable = this.props.disabledAll;
        var showNoneOption = this.props.showNoneOption ? <option value="">無</option> : null;
        var sickLeaves = this.props.sickLeaves ? this.props.sickLeaves : 0;
        var annualLeaves = this.props.annualLeaves ? this.props.annualLeaves : 0;
        var familyCareLeaves = this.props.familyCareLeaves ? this.props.familyCareLeaves : 0;
        if (!sickLeavesOnly)  
        return (
            <select
                id='offType'
                name='offType'
                className={ this.props.className }
                disabled={ isDisable }
                style={ isDisable ? 
                      { ...this.props.style, ...style.type, ...style.disabled } : 
                      {...this.props.style, ...style.type } }
                value={ this.props.value }
                onChange={ this.props.onChange }>
                { showNoneOption }
                <option value="事假" disabled={ this.props.disabled1 }>事假</option>
                <option value="病假" disabled={ !sickLeaves ? true : false }>病假 (剩餘 { sickLeaves }天)</option>
                <option value="喪假" disabled={ this.props.disabled3 }>喪假</option>
                <option value="公假" disabled={ this.props.disabled4 }>公假</option>
                <option value="特休" disabled={ !annualLeaves ? true : false }>特休 (剩餘 { annualLeaves }天)</option>
                <option value="家庭照顧假" disabled={ !familyCareLeaves ? true : false }>家庭照顧假 (剩餘 { familyCareLeaves }天)</option>
                <option value="補休" disabled={ this.props.disabled7 }>補休</option>
                <option value="婚假" disabled={ this.props.disabled8 }>婚假</option>
                <option value="陪產假" disabled={ this.props.disabled9 }>陪產假</option>
                <option value="其他" disabled={ this.props.disabled10 }>其他</option>
            </select>
        ); else return (
            <select
                id='offType'
                name='offType'
                className={ this.props.className }
                disabled={ isDisable }
                style={ isDisable ? 
                      { ...this.props.style, ...style.type, ...style.disabled } : 
                      {...this.props.style, ...style.type } }
                value={ this.props.value }
                onChange={ this.props.onChange }>
                { showNoneOption }
                <option value="病假" disabled={ !sickLeaves ? true : false }>病假 (剩餘 { sickLeaves }天)</option>
                <option value="其他" disabled={ this.props.disabled10 }>其他</option>
            </select>
        );
    }
}

export default OffOption;