import React, {Component} from 'react';
import {connect} from 'react-redux';
import moment from 'moment';

const style = {
    type: { textAlign: 'center' },
    disabled : { backgroundColor: '#ccc', borderColor: '#bbb' }
};
class OffOption extends Component {
    render() {
        return (
            <select
                name='offType'
                className={ this.props.className }
                disabled={ this.props.disabledAll }
                style={ this.props.disabledAll ? 
                      { ...this.props.style, ...style.type, ...style.disabled } : 
                      {...this.props.style, ...style.type } }
                value={ this.props.value }
                onChange={ this.props.onChange }>
                <option value="">無</option>
                <option value="事假" disabled={ this.props.disabled1 }>事假</option>
                <option value="病假" disabled={ this.props.disabled2 }>病假</option>
                <option value="喪假" disabled={ this.props.disabled3 }>喪假</option>
                <option value="公假" disabled={ this.props.disabled4 }>公假</option>
                <option value="特休" disabled={ this.props.disabled5 }>特休</option>
                <option value="家庭照顧假" disabled={ this.props.disabled6 }>家庭照顧假</option>
                <option value="補休" disabled={ this.props.disabled7 }>補休</option>
                <option value="婚假" disabled={ this.props.disabled8 }>婚假</option>
                <option value="陪產假" disabled={ this.props.disabled9 }>陪產假</option>
                <option value="其他" disabled={ this.props.disabled10 }>其他</option>
            </select>
        );
    }
}

export default OffOption;