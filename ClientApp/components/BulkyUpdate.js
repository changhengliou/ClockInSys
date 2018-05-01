import React, {Component} from 'react';
import {connect} from 'react-redux';
import {actionCreators} from '../store/bulkyUpdateStore';
import ReactTable from 'react-table';
import 'react-table/react-table.css';
import {Link} from 'react-router';

class bulkyUpdate extends Component {
    constructor(props) {
        super(props);
        this.renderCells = this.renderCells.bind(this);
        this.columns = [{
                Header: 'No.',
                Cell: props => <span>{ props.index + 1 }</span>,
            }, {
                Header: '日期',
                accessor: 'checkedDate',
                Cell: props => this.renderCells(props),
            }, {
                Header: '姓名',
                accessor: 'userName',
                Cell: props => this.renderCells(props)
            }, {
                Header: '上班時間',
                accessor: 'checkInTime',
                Cell: props => this.renderCells(props, 'isInValid')
            }, {
                Header: '下班時間',
                accessor: 'checkOutTime',
                Cell: props => this.renderCells(props, 'isOutValid')
            }, {
                Header: '上班打卡座標',
                accessor: 'geoLocation1',
                Cell: props => this.renderCells(props)
            }, {
                Header: '下班打卡座標',
                accessor: 'geoLocation2',
                Cell: props => this.renderCells(props)
            }, {
                Header: '加班時間',
                accessor: 'overtimeEndTime',
                Cell: props => this.renderCells(props, 'isOTValid')
            }, {
                Header: '請假申請日期',
                accessor: 'offApplyDate',
                Cell: props => this.renderCells(props, 'isOffValid')
            }, {
                Header: '請假類別',
                accessor: 'offType',
                Cell: props => this.renderCells(props, 'isOffValid')
            }, {
                Header: '請假時間',
                accessor: 'offTime',
                Cell: props => this.renderCells(props, 'isOffValid')
            }, {
                Header: '請假原因',
                accessor: 'offReason',
                Cell: props => this.renderCells(props, 'isOffValid')
            }
        ];
        this.state = {
            fileName: '',
            file: null,
            msg: null
        };
    }

    renderCells (props, option) {
        if (this.props.data.data[props.index]) {
            if (!this.props.data.data[props.index].isDataValid) {
                return <span style={{ color: 'red', fontWeight: 800 }}>{ props.value }</span>;
            }
            if (!option)
                return <span>{ props.value }</span>;
            if (!this.props.data.data[props.index][option]) {
                return <span style={{ color: 'red' }}>{ props.value }</span>;
            }
            return <span>{ props.value }</span>
        }
    }

    render() {
        var props = this.props.data;
        var comp = props.showTable
            ? (
                <div>
                    <div style={{ width: '375px', margin: '0 auto', overflow: 'auto' }}>
                        共: { props.count } 筆資料，
                        錯誤資料: <span style={ props.errorCount ? { color: 'red' } : null }>
                                      { props.errorCount }
                                  </span> 筆
                        { props.errorCount  ? 
                        <div>第 <span style={{ color: 'red' }}>{ props.errorList }</span> 筆有誤</div> :  null } 
                    </div>
                    <ReactTable
                        data={props.data}
                        columns={this.columns}
                        loading={props.isLoading}
                        defaultPageSize={10}
                        showPageSizeOptions={true}
                        previousText='上一頁'
                        nextText='下一頁'
                        loadingText='載入中...'
                        pageText='第'
                        ofText='之'
                        rowsText='筆'
                        noDataText='無資料!'/>
                    <button
                        className='btn_date btn_default'
                        style={{ width: '100px', margin: '10px 0px', color: '#fff'}}
                        onClick={ () => {
                            this.setState({ fileName: null, file: null, msg: null });
                            this.props.submitFile();
                        } }>確認</button>
                    <button
                        className='btn_date btn_danger'
                        style={{ width: '100px', margin: '10px 0px'}}
                        onClick={ () => {
                            this.setState({ fileName: null, file: null, msg: null });
                            this.props.goBackClick();
                        } }>返回</button>
                </div>
            )
            : (
                <div>
                    <div>
                        <span
                            style={{
                            display: 'inline-block',
                            width: '200px',
                            overflow: 'hidden',
                            textOverflow: 'ellipsis',
                            whiteSpace: 'nowrap',
                            verticalAlign: 'middle',
                            textAlign: 'center'
                        }}>
                            {this.state.fileName
                                ? this.state.fileName
                                : '副檔名為xls, xlsx之檔案'}
                        </span>
                        <div
                            className='fileUpload btn_date btn_default'
                            style={{
                            width: '100px',
                            margin: '0px 10px',
                            display: 'inline-block'
                        }}>
                            選擇文件
                            <input
                                className="upload"
                                name="file"
                                type="file"
                                onChange={(e) => {
                                if (e.target.files[0]) {
                                    if (e.target.files[0].size > 3000000) {
                                        this.setState({msg: '檔案過大'});
                                        return;
                                    }
                                    if (e.target.files[0].type !== 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet') {
                                        this.setState({msg: '格式錯誤'});
                                        return;
                                    }
                                    this.setState({fileName: e.target.files[0].name, file: e.target.files, msg: null});
                                } else 
                                    this.setState({fileName: null, file: null});
                                }}
                                accept="application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"/>
                        </div>
                    </div>
                    <div>
                        <button
                            className='btn_date btn_danger'
                            style={{
                            width: '100px',
                            margin: '0px 10px'
                        }}
                            onClick={() => this.props.uploadFile(this.state.file)}
                            type="button">確認上傳</button>
                    </div>
                    <div
                        style={{
                        color: 'red',
                        margin: '8px 0px',
                        fontWeight: '700'
                    }}>{this.state.msg}</div>
                    <span
                        style={{
                        color: 'red',
                        margin: '20px 0px',
                        display: 'inline-block'
                    }}>
                        注意上傳之檔案須符合以下規則
                    </span>
                    <div
                        style={{
                        textAlign: 'left',
                        width: '350px',
                        margin: '0 auto',
                        overflow: 'auto',
                        textOverflow: 'ellipsis',
                        whiteSpace: 'nowrap'
                    }}>
                        <ol>
                            <h4>檔案格式:</h4>
                            <li
                                style={{
                                margin: '6px 0px'
                            }}>Excel工作表副檔名為<span style={{
                    color: 'red'
                }}>xlsx</span>
                            </li>
                            <li
                                style={{
                                margin: '6px 0px'
                            }}>檔案最大為<span style={{
                    color: 'red'
                }}>3MB</span>
                            </li>
                            <h4>資料規則:</h4>
                            <li
                                style={{
                                margin: '6px 0px'
                            }}>單次最多<span style={{
                    color: 'red'
                }}>500筆</span>資料</li>
                            <li
                                style={{
                                margin: '6px 0px'
                            }}>格式須以<Link
                                to='/report'
                                style={{
                    color: 'fuchsia'
                }}>綜合報告</Link>的格式上傳</li>
                            <li style={{ margin: '6px 0px' }}>資料須在工作表的<span style={{
                    color: 'red'
                }}>第一頁</span>
                            </li>
                            <li
                                style={{
                                margin: '6px 0px'
                            }}>A行到K行(Column)依序為日期至請假原因</li>
                            <li
                                style={{
                                margin: '6px 0px'
                            }}>系統由<span style={{
                    color: 'red'
                }}>第三排(Row)</span>開始讀取資料</li>
                            <li
                                style={{
                                margin: '6px 0px'
                            }}>
                                <span
                                    style={{
                                    color: 'red'
                                }}>同用戶同時間已存在之資料會直接複寫</span>
                            </li>
                            <li
                                style={{
                                margin: '6px 0px'
                            }}>所有請假資訊會直接設為<span style={{
                    color: 'red'
                }}>已核准</span>並扣除相對應的請假天數</li>
                        </ol>
                    </div>
                </div>
            );
        return (
            <div style={{ width: '90%', margin: '0 auto' }}>
                <h3 style={{ margin: '8px 0px' }}>上傳檔案以匯入大量資料</h3>
                { comp }
            </div>
        );
    }
}

const mapStateToProps = (state) => {
    return {data: state.bulkyupdate};
}

const mapDispatchToProps = (dispatch) => {
    return {
        uploadFile: (val) => {
            if (val) 
                return dispatch(actionCreators.uploadFile(val));
        },
        goBackClick: () => {
            return dispatch(actionCreators.goBackClick());
        },
        submitFile: () => {
            return dispatch(actionCreators.submitFile());
        }
    }
}

const wrapper = connect(mapStateToProps, mapDispatchToProps);
export default wrapper(bulkyUpdate);
