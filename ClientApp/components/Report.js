import React, { Component } from 'react';
import { connect } from 'react-redux';
import moment from 'moment';
import { actionCreators } from '../store/reportStore';
import Form from './Report.Form';
import Table from './Report.Table';

class Report extends Component {
    render() {
        var props = this.props.data;
        var material;
        switch(props.status){
            case -1:
            default:
                material = <div style={{fontSize: '20px', color: 'red'}}>Something is going down... =(</div>;
                break;
            case 0:
                material = <Form/>;
                break;
            case 1:
                material = <div style={{fontSize: '20px'}}>載入中...</div>;
                break;
            case 2:
                material = <Table/>;
                break;
        }
        var btn = props.status === 0 || props.status === 1 ? null : (
            <div style={{width: '150px', margin: '0 auto', marginTop: '16px'}}>
                <button className='btn_date btn_danger' 
                        onClick={() => this.props.onGoBackClick() }>返回</button>
            </div>
        ); 
        return (
            <div>
                { material }
                { btn }
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
        onGoBackClick: () => {
            dispatch(actionCreators.onGoBackClick());
        }
    }
}

const wrapper = connect(mapStateToProps, mapDispatchToProps);
export default wrapper(Report);
