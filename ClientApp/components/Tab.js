import React, { Component } from 'react';

export default class Tab extends Component {
    constructor(props) {
        super(props);
        this.handleClick = this.handleClick.bind(this);
        var defaultActiveIndex = this.props.defaultActiveIndex ? this.props.defaultActiveIndex : 0;
        this.state = {activeIndex: `key_${defaultActiveIndex}`};
    }

    handleClick(e) {
        this.setState({activeIndex: e.target.getAttribute('data-key')});
    }
    
    render() {
        return (
            <div style={{textAlign: 'left', width: '85%', margin: '0 auto'}}>
                <ul className='tabs'>
                    { 
                        this.props.components.map((obj, i) => {
                            if (`key_${i}` == this.state.activeIndex) {
                                return <li data-key={`key_${i}`} 
                                           className='tabs-list tabs-list-active'
                                           onClick={(e) => this.handleClick(e)}>
                                           {obj.title}
                                        </li> 
                            }
                            return (
                                <li className='tabs-list' 
                                    data-key={`key_${i}`}
                                    onClick={(e) => this.handleClick(e)}>
                                        {obj.title}
                                </li>
                            );
                         })
                    }
                </ul>
                <div className='tabs-content'>
                    {
                        this.props.components.map((obj, i) => {
                            if (`key_${i}` == this.state.activeIndex)
                                return obj.component;
                            else
                                return null;
                        })
                    }
                </div>
                
            </div>
        );
    }
}
