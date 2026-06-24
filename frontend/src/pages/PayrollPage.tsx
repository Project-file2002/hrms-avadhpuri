import { useEffect, useState } from 'react';
import { Table, Button, Modal, Form, Input } from 'antd';
import { PlusOutlined } from '@ant-design/icons';
import api from '../services/api';
import { formatINR, INR_PREFIX } from '../utils/currency';
import { notifySuccess } from '../utils/notification';

interface PayrollStructure {
  id: number;
  name: string;
  description?: string;
  components: { name: string; type: string; amount: number }[];
}

export default function PayrollPage() {
  const [structures, setStructures] = useState<PayrollStructure[]>([]);
  const [loading, setLoading] = useState(false);
  const [modalOpen, setModalOpen] = useState(false);
  const [form] = Form.useForm();

  const fetchStructures = async () => {
    setLoading(true);
    try {
      const res = await api.get('/payroll/structures');
      setStructures(res.data);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => { fetchStructures(); }, []);

  const handleCreate = async (values: { name: string; description?: string }) => {
    await api.post('/payroll/structures', values);
    notifySuccess('Payroll structure created');
    setModalOpen(false);
    form.resetFields();
    fetchStructures();
  };

  const columns = [
    { title: 'Name', dataIndex: 'name', key: 'name' },
    { title: 'Description', dataIndex: 'description', key: 'description' },
    {
      title: 'Components', key: 'components',
      render: (_: unknown, r: PayrollStructure) => r.components?.length ?? 0,
    },
  ];

  return (
    <>
      <div className="page-toolbar">
        <h2 style={{ margin: 0 }}>Payroll Structures</h2>
        <Button type="primary" icon={<PlusOutlined />} onClick={() => setModalOpen(true)}>
          Add Structure
        </Button>
      </div>
      <div className="responsive-table-wrap">
      <Table dataSource={structures} columns={columns} rowKey="id" loading={loading} scroll={{ x: 'max-content' }}
        expandable={{
          expandedRowRender: (r: PayrollStructure) => (
            <Table dataSource={r.components || []} rowKey="name" pagination={false} scroll={{ x: 'max-content' }}
              columns={[
                { title: 'Component', dataIndex: 'name', key: 'name' },
                { title: 'Type', dataIndex: 'type', key: 'type' },
                { title: 'Amount', dataIndex: 'amount', key: 'amount', render: (v: number) => formatINR(v) },
              ]} />
          ),
        }}
      />
      </div>
      <Modal title="Add Structure" open={modalOpen} onCancel={() => setModalOpen(false)} onOk={() => form.submit()}>
        <Form form={form} onFinish={handleCreate} layout="vertical">
          <Form.Item name="name" label="Name" rules={[{ required: true }]}>
            <Input />
          </Form.Item>
          <Form.Item name="description" label="Description">
            <Input.TextArea />
          </Form.Item>
        </Form>
      </Modal>
    </>
  );
}
