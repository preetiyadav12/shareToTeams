import { connect } from 'react-redux';

import ConfigurationScreen from '../components/ConfigurationScreen';
import { addUserToThread, isValidThread } from '../core/sideEffects';

const mapDispatchToProps = (dispatch: any) => ({
  setup: async (displayName: string, emoji: string, acsToken: any, token: any, acsID:string ) => {
    dispatch(addUserToThread(displayName, emoji, acsToken, token, acsID));
  },
  isValidThread: async (threadId: string) => dispatch(isValidThread(threadId))
});

export default connect(undefined, mapDispatchToProps)(ConfigurationScreen);
