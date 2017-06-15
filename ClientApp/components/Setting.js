import React, { Component } from 'react';
import { connect } from 'react-redux';
import { actionCreators } from '../store/settingStore';
import Tab from './Tab';

const components = [{
    title: '預設天數設定', component: <div>Not finish yet.</div>
}, {
    title: '通知設定', component: <div>Not finish yet.</div>
}, {
    title: '電子郵件', component: <div>Not finish yet.</div>
}];

class Setting extends Component {
    constructor(props) {
        super(props);
    }
    
    render() {
        var props = this.props.data;
        return (
            <div>
                <Tab components={components} defaultActiveIndex={1}/>
            </div>
        );
    }
}

const mapStateToProps = (state) => {
    return {
        data: state.setting
    };
}

const mapDispatchToProps = (dispatch) => {
    return {
        
    }
}

const wrapper = connect(mapStateToProps, mapDispatchToProps);
export default wrapper(Setting);
