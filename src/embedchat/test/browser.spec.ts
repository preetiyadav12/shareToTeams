/**
 * * Test Target
 */
import TeamsChat from '../dist/index';

describe('Unit Tests', () => {
  afterEach(() => {
    jest.clearAllMocks();
  });

  describe('constructor', () => {
    // *
    it('It should instantiate the object', async () => {
      const instance = new TeamsChat({
        hostDomain: '',
        apiBaseUrl: '',
        clientId: '',
        tenant: '',
        acsEndpoint: '',
      });

      expect(instance).toBeDefined();
    });
  });
});
